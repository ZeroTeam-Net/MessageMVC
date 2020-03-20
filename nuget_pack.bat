cd C:\Projects\ZeroTeam\MessageMVC\src
dotnet pack ZeroTeam.MessageMVC.sln -c:"Release" -o:"C:\Projects\Agebull\nuget"
del C:\Projects\Agebull\nuget\ZeroTeam.MessageMVC.Kafka.Sample.1.0.0.nupkg
cd c:\projects\agebull\nuget
dotnet nuget push *.nupkg -k oy2gica5n5ejwx6dwbaqimhqo6m7hl5evcnn63oisjnwfy -s https://api.nuget.org/v3/index.json
