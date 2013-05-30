import os
import sys
import re


reasmversion = re.compile(r"""\[assembly: AssemblyVersion\("(?P<version>[0-9\.]+)"\)\]""")
reasmfversion = re.compile(r"""\[assembly: AssemblyFileVersion\("(?P<version>[0-9\.]+)"\)\]""")

asmversion = '[assembly: AssemblyVersion("%s")]'
asmfversion = '[assembly: AssemblyFileVersion("%s")]'

ignores = []
ignores.append("PluginSample")

def updateVersion(vers, build = None):

    v = vers.split('.')
    vbuild = int(v[3])
    if build:
        vbuild = build
    else:
        vbuild = vbuild + 1

    return "%s.%s.%s.%d" % (v[0], v[1], v[2], vbuild)

def updateAssemblyInfo(buildid = None):

    files = os.listdir(".")

    for f in files:
        if os.path.isdir(f):
            asmpath = os.path.join(f, "Properties", "AssemblyInfo.cs")
            ignored = False

            for i in ignores:
                if i in asmpath:
                    ignored = True

            
            if os.path.exists(asmpath) and not ignored:
                print asmpath
                newfiledata = ""
                
                fp = open(asmpath, 'r')

                line = fp.readline()

                while len(line) > 0:
                    m1 = reasmversion.match(line)
                    m2 = reasmfversion.match(line)

                    if m1:
                        vers = m1.group("version")
                        nv = updateVersion(vers, buildid)
                        line = asmversion % (nv)
                        newfiledata = newfiledata + line + '\n'
                        
                    elif m2:
                        vers = m2.group("version")
                        nv = updateVersion(vers, buildid)
                        line = asmfversion % (nv)
                        newfiledata = newfiledata + line + '\n'
                    else:
                        newfiledata = newfiledata + line

                    line = fp.readline()

                fp.close();

                fp = open(asmpath, 'w')
                fp.write(newfiledata)
                fp.close()


if __name__ == "__main__":
    if len(sys.argv) == 1:
        #parsing version number and +1 build
        updateAssemblyInfo()

    else:
        #version no is specified
        nbbuild = int(sys.argv[1])
        updateAssemblyInfo(nbbuild)

