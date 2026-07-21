Environment.SetEnvironmentVariable("DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2SUPPORTSERRORWITHALPN", "false");
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.GastoSmart_Api>("api")
                 .WithHttpEndpoint(port: 5050, name: "http");

var mauiDeploy = builder.AddExecutable(
    "maui-app-deploy", 
    "bash",
    workingDirectory: ".",
    args: [
        "-c", 
        "export JAVA_HOME='/home/tacio/Android/Sdk/jbr' && export PATH=\"$JAVA_HOME/bin:$PATH\" && sleep 5 && (adb uninstall com.companyname.gastosmart || true) && dotnet clean ../GastoSmart.App/GastoSmart.App.csproj && dotnet build ../GastoSmart.App/GastoSmart.App.csproj -f net10.0-android -c Debug && adb install --no-incremental -r -d -t ../GastoSmart.App/bin/Debug/net10.0-android/com.companyname.gastosmart.android-Signed.apk"
    ]
).WithReference(api);

builder.Build().Run();