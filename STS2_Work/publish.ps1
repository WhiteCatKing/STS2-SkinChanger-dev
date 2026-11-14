param([string]$User)
$root='D:\Storage\Desktop\工作2\STS2_Work'
& "$root\steamcmd\steamcmd.exe" +login $User +workshop_build_item "$root\workshop_build.vdf" +quit