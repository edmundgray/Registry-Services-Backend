const { MongoClient } = require('mongodb');

const uri = "mongodb+srv://s00221640:wordpass@omnicluster.k35dhrv.mongodb.net/";
const client = new MongoClient(uri);

class SpecificationService {
  constructor() {
    this.db = null;
  }
  
  async connect() {
    await client.connect();
    this.db = client.db("registry");
  }
  
  async close() {
    await client.close();
  }
  
  async getAllSpecifications(filters = {}) {
    const collection = this.db.collection("specifications");
    let query = {};
    
    if (filters.sector) query.Sector = filters.sector;
    if (filters.country) query.Country = filters.country;
    if (filters.search) query.$text = { $search: filters.search };
    
    return await collection.find(query)
      .project({
        SpecificationIdentifier: 1,
        SpecificationName: 1,
        Purpose: 1,
        Sector: 1,
        Country: 1,
        DateOfImplementation: 1,
        GoverningEntity: 1,
        UnderlyingSpecification: 1,
        status: 1
      })
      .toArray();
  }
  
  async getSpecificationWithModel(specId) {
    const specifications = this.db.collection("specifications");
    const invoiceCore = this.db.collection("InvoiceCore");
    const extensionData = this.db.collection("ExtensionComponentData");
    
    const spec = await specifications.findOne({ SpecificationIdentifier: specId });
    if (!spec) return null;
    
    let model = null;
    
    if (spec.modelType === "core") {
      const coreElements = await invoiceCore.find({
        id: { $in: spec.linkedElements }
      }).toArray();
      model = { type: "core", elements: coreElements };
    } else if (spec.modelType === "extension") {
      const extensionModel = await extensionData.findOne({
        "extensionComponent.id": specId
      });
      model = { type: "extension", elements: extensionModel };
    }
    
    return { ...spec, model: model };
  }
  
  async advancedSearch(searchParams) {
    const collection = this.db.collection("specifications");
    let query = {};
    
    if (searchParams.elementId) query.linkedElements = searchParams.elementId;
    if (searchParams.elementType) {
      if (searchParams.elementType === "core") query.modelType = "core";
      if (searchParams.elementType === "extension") query.modelType = "extension";
    }
    if (searchParams.sector) query.Sector = searchParams.sector;
    if (searchParams.country) query.Country = searchParams.country;
    
    return await collection.find(query).toArray();
  }
  
  async getMySpecifications(submitterId, status = null) {
    const collection = this.db.collection("specifications");
    let query = { submitterId: submitterId };
    if (status) query.status = status;
    
    return await collection.find(query)
      .sort({ createdDate: -1 })
      .toArray();
  }
  
  async updateSpecificationStatus(specId, newStatus) {
    const collection = this.db.collection("specifications");
    return await collection.updateOne(
      { SpecificationIdentifier: specId },
      { 
        $set: { 
          status: newStatus,
          lastModified: new Date()
        }
      }
    );
  }
}

module.exports = SpecificationService;