# File structure

- 4 bytes: TiNy
- an arbitrary amount of sections:
  - 4 bytes: section identifier
  - 4 bytes (be): section content length
  - x bytes: section content

# Section types

- meta
- texture
- line

## Metadata

- 4 bytes: MeTa
- 4 bytes: length (16 bytes)
- 3 bytes: reserved, always zero
- 1 byte: number of palette entries
- 2 bytes (be): width of a texture tile in pixels
- 2 bytes (be): height of a texture tile in pixels
- 1 byte: number of texture tile columns
- 1 byte: number of texture tile rows
- 2 bytes (be): number of characters
- 2 bytes (be): number of lines
- 2 bytes (be): max line length

## Texture

- 4 bytes: TxTr
- 4 bytes: length (png size)
- x bytes: png

## Line

- 4 bytes: LiNe
- 4 bytes: length (2 x number of characters)
- for each character:
  - 1 byte: x offset into tile
  - 1 byte: y offset into tile
