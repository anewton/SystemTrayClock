### How to convert svg to ico
- Install Inkscape
- Install ImageMagik
    - include legacy convert tool in setup
- Edit svg in Inkscape
    - set document properties to resize to the image dimensions
- Convert the svg to an ico file using the command line tool magick
     ```
        > magick convert -background none icon.svg -define icon:auto-resize icon.ico
     ```