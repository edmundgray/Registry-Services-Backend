const { MongoClient } = require('mongodb');

const uri = "mongodb+srv://s00221640:wordpass@omnicluster.k35dhrv.mongodb.net/";
const client = new MongoClient(uri);

async function setupSpecifications() {
  try {
    await client.connect();
    const db = client.db("registry");
    const collection = db.collection("specifications");
    
    const sampleSpecs = [
      {
        "SpecificationIdentifier": "OD002",
        "SpecificationName": "Delivery Information Extension",
        "GoverningEntity": "Irish Tax Authority",
        "ContactInformation": "ireland@tax.ie",
        "Sector": "Manufacturing",
        "SubSector": "Food Production",
        "Country": "IE",
        "Purpose": "Provide delivery location and party details",
        "SpecificationVersion": "1.0",
        "DateOfImplementation": "2025-01-01",
        "CoreVersion": "EN 16931-1:2017",
        "UnderlyingSpecification": "Extension",
        "SourceLink": "https://example.ie/od002-spec",
        "PreferredSyntax": "UBL 2.1",
        "Core": { "ID": "", "Cardinality": "", "UsageNote": "", "TypeOfChange": "" },
        "Extension": {
          "ID": "XG-1",
          "Cardinality": "0..1",
          "UsageNote": "Optional delivery information group",
          "Justification": "Required for Irish manufacturing compliance",
          "TypeOfExtension": "Business group with delivery party details"
        },
        "linkedElements": ["XT-13", "XT-14", "XT-19"],
        "modelType": "extension",
        "createdDate": new Date(),
        "status": "submitted"
      },
      {
        "SpecificationIdentifier": "CORE001",
        "SpecificationName": "Standard Invoice Core",
        "GoverningEntity": "European Committee",
        "ContactInformation": "info@cen.eu",
        "Sector": "General",
        "SubSector": "All",
        "Country": "EU",
        "Purpose": "Standard invoice format for European Union",
        "SpecificationVersion": "2017",
        "DateOfImplementation": "2017-01-01",
        "CoreVersion": "EN 16931-1:2017",
        "UnderlyingSpecification": "Core",
        "SourceLink": "https://cen.eu/standard",
        "PreferredSyntax": "UBL 2.1",
        "Core": {
          "ID": "BG-1",
          "Cardinality": "1..1",
          "UsageNote": "All core elements included",
          "TypeOfChange": "none"
        },
        "Extension": { "ID": "", "Cardinality": "", "UsageNote": "", "Justification": "", "TypeOfExtension": "" },
        "linkedElements": ["BT-1", "BT-2", "BT-3", "BG-1", "BG-2"],
        "modelType": "core",
        "createdDate": new Date(),
        "status": "submitted"
      }
    ];
    
    await collection.deleteMany({});
    const result = await collection.insertMany(sampleSpecs);
    console.log(`${result.insertedCount} specifications inserted`);
    
    await collection.createIndex({ "SpecificationIdentifier": 1 });
    await collection.createIndex({ "SpecificationName": "text", "Purpose": "text" });
    await collection.createIndex({ "linkedElements": 1 });
    await collection.createIndex({ "Sector": 1, "Country": 1 });
    
    console.log("Indexes created");
    
  } catch (error) {
    console.error("Error:", error);
  } finally {
    await client.close();
  }
}

setupSpecifications();
