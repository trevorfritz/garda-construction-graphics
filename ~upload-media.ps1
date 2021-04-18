$env:Path += ";c:\dev\garda-construction-graphics\tools\azcopy"
$storage = "https://gardadev.blob.core.windows.net/"
$sas = "?sv=2020-02-10&ss=b&srt=co&sp=rwdlacx&se=2021-04-30T07:59:53Z&st=2021-04-16T23:59:53Z&spr=https&sig=RfggBcfLsI%2FhLyfbhG0WzVVQVZJq26LRqFQMkWGkbzs%3D"
#azcopy remove ($storage + "media" + $sas) --recursive=true
azcopy sync .\web\media "https://gardadev.blob.core.windows.net/media2?sv=2020-02-10&ss=b&srt=co&sp=rwdlacx&se=2021-04-30T07:59:53Z&st=2021-04-16T23:59:53Z&spr=https&sig=RfggBcfLsI%2FhLyfbhG0WzVVQVZJq26LRqFQMkWGkbzs%3D" 

#azcopy reference:
#https://docs.microsoft.com/en-us/azure/storage/common/storage-ref-azcopy-copy?toc=/azure/storage/blobs/toc.json