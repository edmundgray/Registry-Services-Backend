const { MongoClient } = require('mongodb');

// Test different connection string formats
const testConnections = [
  "mongodb+srv://s00221640:pSebFzvXh0kzKuGt@omnicluster.k35dhrv.mongodb.net/",
  "mongodb+srv://s00221640:pSebFzvXh0kzKuGt@omnicluster.k35dhrv.mongodb.net/registry",
  "mongodb+srv://s00221640:pSebFzvXh0kzKuGt@omnicluster.k35dhrv.mongodb.net/?retryWrites=true&w=majority"
];

async function testConnection(uri, index) {
  const client = new MongoClient(uri);
  
  try {
    console.log(`\nTesting connection ${index + 1}...`);
    console.log(`URI: ${uri.replace(/:[^:]+@/, ':***@')}`);
    
    await client.connect();
    console.log('✅ Connection successful!');
    
    // Test database access
    const db = client.db("registry");
    const collections = await db.listCollections().toArray();
    console.log(`Found ${collections.length} collections`);
    
    return true;
  } catch (error) {
    console.log('❌ Connection failed:', error.message);
    return false;
  } finally {
    await client.close();
  }
}

async function runTests() {
  console.log('Testing MongoDB connections...\n');
  
  for (let i = 0; i < testConnections.length; i++) {
    const success = await testConnection(testConnections[i], i);
    if (success) {
      console.log(`\n✅ Use this connection string in your files:`);
      console.log(`"${testConnections[i]}"`);
      break;
    }
  }
}

runTests();
