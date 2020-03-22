cd C:\Projects\ZeroTeam\MessageMVC
cd nuget
del *.nupkg
cd ..
dotnet pack ZeroTeam.MessageMVC.sln -c:"Release" -o:"C:\Projects\ZeroTeam\MessageMVC\nuget"
cd nuget
del *.1.0.0.nupkg
dotnet nuget push *.nupkg -k oy2gica5n5ejwx6dwbaqimhqo6m7hl5evcnn63oisjnwfy -s https://api.nuget.org/v3/index.json
