Candy Bitmap Font – Unity Ready
================================

Isi paket:
- CandyBitmapFontAtlas.png     -> atlas karakter transparan
- CandyBitmapFontMap.json      -> mapping A-Z, 0-9, +, -, =, plus spacing metrics
- CandyBitmapFont.fnt          -> file BMFont text format
- Editor/CandyBitmapFontImporter.cs -> menu editor untuk slice atlas otomatis
- Scripts/CandyBitmapTextUGUI.cs    -> komponen UI fallback / runtime bitmap text

Karakter yang tersedia:
A-Z, 0-9, +, -, =, dan spasi (spasi virtual)

METRICS DEFAULT
- Font Size   : 160
- Line Height : 192
- Baseline    : 152
- Space Width : 80
- Tracking    : sekitar 12 px (lebih lebar untuk I/J/1/+/-/=)

CARA PAKAI – OPSI 1 (PALING MUDAH, CUSTOM UI COMPONENT)
1. Copy folder ini ke Assets/CandyBitmapFont/
2. Di Unity, pilih CandyBitmapFontAtlas.png lalu set:
   - Alpha Is Transparency = ON
   - Mip Maps = OFF
3. Buat GameObject UI kosong di Canvas.
4. Add Component -> Candy Bitmap Text
5. Isi field:
   - Atlas Texture  = CandyBitmapFontAtlas.png
   - Font Map Json  = CandyBitmapFontMap.json
   - Text           = misalnya SCORE 123
6. Atur fontSize / extraLetterSpacing sesuai kebutuhan.

CARA PAKAI – OPSI 2 (SLICE SPRITES)
1. Copy folder ini ke Assets/CandyBitmapFont/
2. Di Unity buka menu: Tools -> Candy Bitmap Font -> Configure Atlas Slices
3. Atlas akan otomatis dislice jadi sprite per karakter.
4. Ini berguna kalau kamu mau akses sprite satu-satu secara manual.

CARA PAKAI – OPSI 3 (COBA IMPORT BMFONT KE TMP)
1. Pastikan TextMeshPro sudah terinstall.
2. Copy CandyBitmapFont.fnt dan CandyBitmapFontAtlas.png ke project.
3. Di beberapa versi Unity/TMP, file BMFont bisa dipakai untuk membuat TMP bitmap font asset.
4. Kalau versi Unity/TMP kamu tidak mendukung import BMFont langsung, pakai Opsi 1 sebagai fallback.

CATATAN
- Input lowercase sebaiknya akan diubah ke uppercase oleh script (forceUppercase = true).
- Spasi tidak ada sprite-nya, tapi width-nya sudah didefinisikan di JSON/FNT.
- Kalau mau lebih rapat / lebih renggang, ubah extraLetterSpacing di komponen.
- Karena ini bitmap/color glyph, efek warna, outline, dan glossy akan tetap terjaga.


V3 FIX — Unity 6 / texture 1024 error
------------------------------------
If you saw:
ArgumentException: Could not create sprite (...) from a 1024x1024 texture

Cause: the JSON coordinates are based on the original 1344x1152 atlas, while Unity imported/downscaled it to 1024.

V3 fixes this in TWO ways:
1. Editor importer forces Max Size 2048 + NPOT Scale None.
2. Runtime component automatically scales glyph coordinates to the real imported Texture2D size, so it still works even if Unity downsizes the atlas.

You do NOT have to convert the texture to a Sprite first when using CandyBitmapTextUGUI. Assign the atlas Texture2D directly.
