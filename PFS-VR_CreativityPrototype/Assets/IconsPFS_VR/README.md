Icons taken from: https://feathericons.com/

Conversion from SVG to PNG: `magick mogrify -channel RGB -negate *.png`
> ATTENTION: mogrify modifies __in place__

Batch prefixing of filename with windows powershell: `(Get-ChildItem -File) | Rename-Item -NewName {$_.Name -replace "^","i_"}`