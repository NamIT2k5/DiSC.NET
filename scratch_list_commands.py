import re

with open(r'd:\NCKH\4.Act\DiSC.NET-main\NetConsole\Program.cs', encoding='utf-8') as f:
    prog_lines = f.readlines()
with open(r'd:\NCKH\4.Act\DiSC.NET-main\NetworkRobustness\Lib\CommandFunction.cs', encoding='utf-8') as f:
    cmd_lines = f.readlines()

cmds = []
for line in prog_lines[22:40]:
    s = line.strip()
    if s.startswith('//'): continue
    m = re.search(r'new TextCommand\(\s*(?:true|false)\s*,\s*"([^"]+)"', s)
    if m:
        cmds.append(('sys', m.group(1), s))

for line in cmd_lines:
    s = line.strip()
    if s.startswith('//'): continue
    m = re.search(r'new TextCommand\(\s*(?:true|false)\s*,\s*"([^"]+)"', s)
    if m:
        cmds.append(('app', m.group(1), s))

for idx, (typ, name, raw) in enumerate(cmds, 1):
    print(f'{idx:3d}: [{typ}] {name}')
