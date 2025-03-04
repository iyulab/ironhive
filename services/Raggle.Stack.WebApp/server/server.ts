import express from 'express';
import path from 'path';

const app = express();
const PORT = 3000;

// Vite 빌드된 파일 제공
app.use(express.static(path.join(__dirname, '../../dist')));

app.get('*', (_req, res) => {
  res.sendFile(path.resolve(__dirname, '../../dist/index.html'));
});

app.listen(PORT, () => {
  console.log(`Server is running at http://localhost:${PORT}`);
});
