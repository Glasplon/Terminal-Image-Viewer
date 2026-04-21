More will come here.

this is a project I made so I could view images in folders easier, in situations where just viewing image might not work.

find . -name "*.jpg" -print0 | xargs -0 -n 1 dotnet run 100 c b
