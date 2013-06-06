import os
import shutil

dirs = []
dirs.append("deploy");
dirs.append("deploy/ServerPlugins")
dirs.append("deploy/Terraria")
dirs.append("deploy/tmapper")
dirs.append("deploy/tmapper/css")
dirs.append("deploy/tmapper/js")
dirs.append("deploy/tmapper/img")

for d in dirs:
    if not os.path.exists(d):
        os.mkdir(d)

def copyFolder(src, dest):
    files = os.listdir(src)

    for f in files:
        if f.endswith('.png'):
            fname = os.path.basename(f)
            fdest = os.path.join(dest, fname)
            shutil.copy(os.path.join(src,f), fdest)

shutil.copy("TerrariaServerBins/TerrariaServer.exe", "deploy/TerrariaServer.exe")
shutil.copy("TPulseAPI/bin/Release/TPulseAPI.dll","deploy/ServerPlugins/TPulseAPI.dll")
shutil.copy("TFriends/bin/Release/TFriends.dll", "deploy/ServerPlugins/TFriends.dll")
shutil.copy("TChestControl/bin/Release/TChestControl.dll", "deploy/ServerPlugins/TChestControl.dll")
shutil.copy("TChatChannels/bin/Release/TChatChannels.dll", "deploy/ServerPlugins/TChatChannels.dll")
shutil.copy("TMapper/bin/Release/TMapper.dll", "deploy/ServerPlugins/TMapper.dll")

shutil.copy("HttpBins/HttpServer.dll", "deploy/ServerPlugins/HttpServer.dll")
shutil.copy("HttpBins/HttpServer.xml", "deploy/ServerPlugins/HttpServer.xml")

shutil.copy("SqlBins/Mono.Data.Sqlite.dll", "deploy/ServerPlugins/Mono.Data.Sqlite.dll")
shutil.copy("SqlBins/MySql.Data.dll", "deploy/ServerPlugins/MySql.Data.dll")
shutil.copy("SqlBins/MySql.Web.dll", "deploy/ServerPlugins/MySql.Web.dll")
shutil.copy("SqlBins/sqlite3.dll", "deploy/sqlite3.dll")
shutil.copy("TPulseAPI/Newtonsoft.Json.dll", "deploy/ServerPlugins/Newtonsoft.Json.dll")


#TMapper web apps

shutil.copy('TMapper/WebApps/index.html', 'deploy/tmapper/index.html')
shutil.copy('TMapper/WebApps/js/legends.js', 'deploy/tmapper/js/legends.js')
shutil.copy('TMapper/WebApps/js/tmapper.js', 'deploy/tmapper/js/tmapper.js')
#shutil.copy('TMapper/WebApps/js/waypoint.js', 'deploy/tmapper/js/waypoint.js')
shutil.copy('TMapper/WebApps/css/tmapper.css', 'deploy/tmapper/css/tmapper.css')
shutil.copy('TMapper/WebApps/img/waypoint.png', 'deploy/tmapper/img/waypoint.png')
copyFolder( 'TMapper/Resources', 'deploy/tmapper/img') 
