in memory DataProvider 

A simple and threadsafe in moemory data provider which does not have any references to other packages.
should only be used for testing
does not persist data between instances and must therefore be provided as singleton
use the ServiceCollection.AddInMemoryDataProvider method to add them to your di.
