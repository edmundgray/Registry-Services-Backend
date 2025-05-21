const express = require('express');
const cors = require('cors');
const SpecificationService = require('./specification-queries');

const app = express();
const port = process.env.PORT || 3001;

app.use(cors());
app.use(express.json());

const specService = new SpecificationService();

async function initializeApp() {
  try {
    await specService.connect();
    console.log('Connected to MongoDB');
  } catch (error) {
    console.error('Failed to connect:', error);
    process.exit(1);
  }
}

app.get('/api/specifications', async (req, res) => {
  try {
    const filters = {
      sector: req.query.sector,
      country: req.query.country,
      search: req.query.search
    };
    const specifications = await specService.getAllSpecifications(filters);
    res.json(specifications);
  } catch (error) {
    console.error('Error getting specifications:', error);
    res.status(500).json({ error: 'Failed to get specifications' });
  }
});

app.get('/api/specifications/:id', async (req, res) => {
  try {
    const specWithModel = await specService.getSpecificationWithModel(req.params.id);
    if (!specWithModel) {
      return res.status(404).json({ error: 'Specification not found' });
    }
    res.json(specWithModel);
  } catch (error) {
    console.error('Error getting specification:', error);
    res.status(500).json({ error: 'Failed to get specification' });
  }
});

app.post('/api/specifications/search/advanced', async (req, res) => {
  try {
    const results = await specService.advancedSearch(req.body);
    res.json(results);
  } catch (error) {
    console.error('Error in advanced search:', error);
    res.status(500).json({ error: 'Failed to perform advanced search' });
  }
});

app.get('/api/my-specifications/:submitterId', async (req, res) => {
  try {
    const mySpecs = await specService.getMySpecifications(req.params.submitterId, req.query.status);
    res.json(mySpecs);
  } catch (error) {
    console.error('Error getting my specifications:', error);
    res.status(500).json({ error: 'Failed to get my specifications' });
  }
});

app.patch('/api/specifications/:id/status', async (req, res) => {
  try {
    const { status } = req.body;
    const result = await specService.updateSpecificationStatus(req.params.id, status);
    
    if (result.matchedCount === 0) {
      return res.status(404).json({ error: 'Specification not found' });
    }
    res.json({ message: 'Status updated successfully' });
  } catch (error) {
    console.error('Error updating status:', error);
    res.status(500).json({ error: 'Failed to update status' });
  }
});

app.get('/api/specifications/filters', async (req, res) => {
  try {
    const filters = {
      sectors: ['Manufacturing', 'Services', 'Retail', 'Healthcare'],
      countries: ['IE', 'DE', 'FR', 'NL', 'BE'],
      types: ['Core', 'Extension', 'CIUS']
    };
    res.json(filters);
  } catch (error) {
    res.status(500).json({ error: 'Failed to get filters' });
  }
});

initializeApp().then(() => {
  app.listen(port, () => {
    console.log(`Server running on port ${port}`);
  });
});

process.on('SIGTERM', async () => {
  await specService.close();
  process.exit(0);
});
