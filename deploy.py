import os
import shutil

if not os.path.exists("deploy"):
    os.mkdir("deploy")
    os.mkdir("deploy/ServerPlugins")
    os.mkdir("deploy/Terraria")


shutil.copy("TerrariaServerBins/TerrariaServer.exe", "deploy/TerrariaServer.exe")
shutil.copy("TPulseAPI/bin/Release/TPulseAPI.dll","deploy/ServerPlugins/TPulseAPI.dll")
shutil.copy("TFriends/bin/Release/TFriends.dll", "deploy/ServerPlugins/TFriends.dll")
shutil.copy("TChatChannels/bin/Release/TChatChannels.dll", "deploy/ServerPlugins/TChatChannels.dll")

shutil.copy("HttpBins/HttpServer.dll", "deploy/ServerPlugins/HttpServer.dll")
shutil.copy("HttpBins/HttpServer.xml", "deploy/ServerPlugins/HttpServer.xml")

shutil.copy("SqlBins/Mono.Data.Sqlite.dll", "deploy/ServerPlugins/Mono.Data.Sqlite.dll")
shutil.copy("SqlBins/MySql.Data.dll", "deploy/ServerPlugins/MySql.Data.dll")
shutil.copy("SqlBins/MySql.Web.dll", "deploy/ServerPlugins/MySql.Web.dll")
shutil.copy("SqlBins/sqlite3.dll", "deploy/sqlite3.dll")
shutil.copy("TPulseAPI/Newtonsoft.Json.dll", "deploy/ServerPlugins/Newtonsoft.Json.dll")

