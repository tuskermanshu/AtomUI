#!/usr/bin/env python3
"""Explicit, heavyweight ILC8 mechanism verification. Not part of the global test suite."""
import argparse
import hashlib
import json
import os
import pathlib
import re
import shutil
import struct
import subprocess
import sys
import tempfile

FIXTURES = pathlib.Path(__file__).resolve().parent / 'fixtures'
CASES = {
    'ALL': ['a','b','direct','generic','root1','root2','style','token'],
    'DIRECT':['direct'], 'TOKEN':['token'], 'STYLE':['style'], 'TRANSITIVE':['a','b'],
    'CYCLE':['root1','root2'], 'GENERIC':['generic'], 'STATIC':[], 'NONE':[], 'OFF':[],
    'CCTOR':['a','b'], 'SAME_NAME':[], 'OR':['direct','direct-alias'],
    'ARRAY':['direct'], 'TYPEOF':['direct'], 'THROW':['throw'], 'CCTOR_THROW':['throw'],
    'SECOND':[], 'BOTH':['direct']
}


def run_logged(command, log, **kwargs):
    with log.open('w') as stream:
        result = subprocess.run(command, stdout=stream, stderr=subprocess.STDOUT, **kwargs)
    if result.returncode:
        raise RuntimeError(str(command) + '\n' + log.read_text()[-6000:])


def macho_symbols(path):
    data = path.read_bytes()
    magic, _, _, _, ncmds, _, _, _ = struct.unpack_from('<8I', data)
    if magic != 0xfeedfacf:
        raise RuntimeError('This verifier currently requires a little-endian 64-bit Mach-O image')
    sections = []
    symtab = None
    offset = 32
    for _ in range(ncmds):
        command, size = struct.unpack_from('<II', data, offset)
        if command == 0x19:
            count = struct.unpack_from('<I', data, offset + 64)[0]
            for index in range(count):
                entry = offset + 72 + index * 80
                name = data[entry:entry+16].split(b'\0')[0].decode()
                segment = data[entry+16:entry+32].split(b'\0')[0].decode()
                address, length, file_offset = struct.unpack_from('<QQI', data, entry+32)
                sections.append((name, segment, address, length, file_offset))
        elif command == 2:
            symtab = struct.unpack_from('<4I', data, offset+8)
        offset += size
    symbol_offset, count, string_offset, _ = symtab
    symbols = {}
    for index in range(count):
        string_index, kind, section, _, value = struct.unpack_from('<IBBHQ', data, symbol_offset+16*index)
        if string_index and kind & 0xe == 0xe and not kind & 0xe0:
            end = data.index(0, string_offset+string_index)
            name = data[string_offset+string_index:end].decode()
            symbols[name] = (value, section)
    return data, sections, symbols


def verify_native_table(executable, report, group, symbols, data, sections):
    table_name = '_' + group['SymbolName']
    if report['Scanner'] or not group['Requested']:
        assert table_name not in symbols, 'Unexpected native dispatch table'
        return None
    address, section_id = symbols[table_name]
    section = sections[section_id-1]
    assert section[1] != '__TEXT', 'Absolute pointers must not require loader writes to __TEXT'
    position = address-section[2]+section[4]
    version, width, count, reserved = struct.unpack_from('<4I', data, position)
    assert (version,width,count,reserved) == (1,8,group['UniqueMethods'],0)
    # dyld_info decodes the actual linked chained fixups. Raw bytes on modern macOS
    # contain chain encoding, not ready-to-call process addresses.
    output = subprocess.check_output(['xcrun','dyld_info','-fixups',str(executable)],text=True,stderr=subprocess.STDOUT)
    rebases = {}
    for line in output.splitlines():
        match = re.search(r'(0x[0-9A-Fa-f]+)\s+rebase\s+(0x[0-9A-Fa-f]+)',line)
        if match:
            rebases[int(match[1],16)] = int(match[2],16)
    actual = [rebases[address+16+8*index] for index in range(count)]
    expected = [symbols['_'+name][0] for name in group['SelectedMethodSymbols']]
    assert actual == expected, ('Native table target/order mismatch',actual,expected)
    return {'section':section[1]+','+section[0],'entries':count,'version':version,'pointerWidth':width,'exactRelocations':True}


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--compiler-output',type=pathlib.Path,required=True)
    parser.add_argument('--work-root',type=pathlib.Path)
    parser.add_argument('--keep-artifacts',action='store_true')
    parser.add_argument('--cases',nargs='+',choices=list(CASES),default=list(CASES))
    parser.add_argument('--rids',nargs='+',default=['osx-arm64'])
    parser.add_argument('--no-optimization-crosscheck',action='store_true')
    args=parser.parse_args()
    root=(args.work_root or pathlib.Path(tempfile.mkdtemp(prefix='atomui-test-ilc8-p1-'))).resolve()
    root.mkdir(parents=True,exist_ok=True)
    logs=root/'logs'; logs.mkdir(exist_ok=True)
    compiler=args.compiler_output.resolve()
    fixture=root/'fixture'
    components=fixture/'components'; app=fixture/'app'
    components.mkdir(parents=True,exist_ok=True); app.mkdir(parents=True,exist_ok=True)
    for name,dest in [('Components.cs.txt',components/'Components.cs'),('Components.csproj.txt',components/'P1.Components.csproj'),('Program.cs.txt',app/'Program.cs'),('App.csproj.txt',app/'P1.App.csproj'),('map.json',root/'map.json')]:
        shutil.copyfile(FIXTURES/name,dest)
    run_logged(['dotnet','build','-c','Release'],logs/'components-build.log',cwd=components)
    results=[]
    configurations=[(case,scanner,True,rid) for rid in args.rids for case in args.cases for scanner in (False,True)]
    if not args.no_optimization_crosscheck:
        configurations += [(case,scanner,False,rid) for rid in args.rids for case in ('ALL','CCTOR','OFF','STATIC') if case in args.cases for scanner in (False,True)]
    try:
        for case,scanner,optimize,rid in configurations:
            label=f'{rid}-{case}-scan{int(scanner)}-opt{int(optimize)}'
            directory=root/'out'/label
            report_path=logs/(label+'-selection.json')
            env=dict(os.environ,ATOMUI_ILC8_MANIFEST=str(root/'map.json'),ATOMUI_ILC8_REPORT=str(report_path))
            command=['dotnet','publish','-c','Release','-r',rid,f'-p:Case={case}',f'-p:NoScanner={str(not scanner).lower()}',f'-p:Optimize={str(optimize).lower()}',f'-p:IlcToolsPath={compiler}/tools/',f'-p:BaseIntermediateOutputPath={root}/obj/{label}/',f'-p:BaseOutputPath={root}/bin/{label}/','-o',str(directory)]
            run_logged(command,logs/(label+'-publish.log'),cwd=app,env=env)
            report=json.loads(report_path.read_text())
            assert report['HostRuntime'].startswith('.NET 10.'),report['HostRuntime']
            assert report['Scanner']==scanner
            executable=directory/'AtomUIRegistrationProbe'
            data,sections,symbols=macho_symbols(executable)
            tables=[]
            assert {g['Id'] for g in report['Groups']}=={'P1.Components/default','P1.Components/second'}
            for group in report['Groups']:
                second=group['Id']=='P1.Components/second'
                expected=(['direct'] if case in ('SECOND','BOTH') else []) if second else CASES[case]
                requested=case in ('SECOND','BOTH') if second else case not in ('OFF','SECOND')
                assert group['Selected']==expected,(label,group['Id'],group['Selected'],expected)
                assert group['Requested']==requested,(label,group['Id'])
                selected=set(group['SelectedMethodSymbols'])
                for condition in group['Conditions']:
                    assert ('_'+condition['MethodSymbol'] in symbols)==(condition['MethodSymbol'] in selected),(label,condition)
                tables.append({'group':group['Id'],'table':verify_native_table(executable,report,group,symbols,data,sections)})
            if case in ('ALL','STATIC'):
                assert any('StaticOnly__Calculate' in name for name in symbols)
            sentinel='UNUSED_FACTORY_SENTINEL_9DBC2310'
            assert sentinel.encode() not in data and sentinel.encode('utf-16le') not in data
            runtime_expected={'OR':['direct'],'CCTOR_THROW':[],'SECOND':['second'],'BOTH':['direct','second']}.get(case,CASES[case])
            actual=subprocess.run([str(executable)],env=dict(os.environ,P1_EXPECT=','.join(runtime_expected)),capture_output=True,text=True)
            execution={'exitCode':actual.returncode,'stdout':actual.stdout,'stderr':actual.stderr}
            (logs/(label+'-run.json')).write_text(json.dumps(execution,indent=2))
            assert actual.returncode==0,(label,execution)
            assert 'runtime=.NET 8.0.27' in actual.stdout,execution
            result={'case':case,'rid':rid,'scanner':scanner,'optimize':optimize,'host':report['HostRuntime'],'selected':{g['Id']:g['Selected'] for g in report['Groups']},'nativeTables':tables,'nativeSha256':hashlib.sha256(data).hexdigest(),'runtimeExit':actual.returncode,'runtime':'.NET 8.0.27'}
            results.append(result)
            (logs/'matrix-results.json').write_text(json.dumps(results,indent=2))
            print(json.dumps(result),flush=True)
        print(f'PASS NativeAOT mechanism matrix {len(results)}; evidence={logs}',flush=True)
    except Exception:
        print('FAILED: evidence and runnable artifacts retained at '+str(root),file=sys.stderr)
        raise
    if not args.keep_artifacts:
        for name in ('fixture','obj','bin','out'):
            shutil.rmtree(root/name,ignore_errors=True)
        (root/'map.json').unlink(missing_ok=True)
    return 0


if __name__=='__main__':
    sys.exit(main())
