# Overview JLib.DataGeneration
## Deterministic Id Generator
- provides a way to use readable string identifier instead of integer/guid ids to handle Ids
- currently supports int, guid & string
- the ids are deterministic each time they are retrieved, and therefore compatible with snapshot tests
- SHOULD be used for testing only
## Package Based Data Generator
- allows you to define data as packages which reference each other
