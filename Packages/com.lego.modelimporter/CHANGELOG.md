# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.4.1] - 2021-07-06

### Added
- Import of individual parts

### Changed
- Updated brick database
- Updated dependencies and required Unity version

## [0.4.0] - 2021-05-06

### Fixed
- Issue with chamfer generation when part submeshes were not in local origo
- Issue with chamfer generation between shells and colour change surface submeshes of parts
- Issue with mesh stitching during chamfer generation

### Changed
- All part submeshes are now placed in local origo of the part 

## [0.3.12] - 2021-05-03

### Changed
- Transparent materials are now alpha blended

## [0.3.11] - 2021-04-19

### Fixed
- Pivot recomputation on reimport
- Build issue when including deleted scenes

### Changed
- Sample scene demonstrating runtime brick building

## [0.3.10] - 2021-04-13

### Fixed
- Performance issue with updating assets 

## [0.3.9] - 2021-04-12

### Fixed
- Placement of a number of bricks
- Issue with updating assets that have no colliders 

## [0.3.8] - 2021-04-08

### Fixed
- Issue with connectivity detection when importing

### Added
- Warning when using assemblies that have missing parts

## [0.3.7] - 2021-03-29

### Added
- Stud.io palette generator with fully and partially supported connectivity

### Fixed
- Pivot computation when building sideways

## [0.3.6] - 2021-03-12

### Fixed
- Placement of a number of bricks

## [0.3.5] - 2021-03-04

### Fixed
- Compile error when building

## [0.3.4] - 2021-03-03

### Added
- DUPLO common parts

### Changed
- Updated brick database

## [0.3.3] - 2021-02-24

### Added
- Axle connectivity
- Fixed connectivity

### Changed
- Updated connectivity system
- Bricks will now ghost when not able to be placed while building

## [0.3.0] - 2021-02-01

### Changed
- Updated to Unity 2020.2

### Fixed
- Crash when undoing in Unity 2020.2

## [0.1.0] - 2020-02-03

### This is the first release of *\<LEGO Model Importer\>*.

*Short description of this release*
