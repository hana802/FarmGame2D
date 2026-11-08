const fs = require('fs');
const { chromium } = require('playwright');

const gameUrl = process.argv[2] || 'http://127.0.0.1:8765/';
const outputPath = 'Builds/UI-QA/toast-safe-area.png';

(async () => {
  fs.mkdirSync('Builds/UI-QA', { recursive: true });
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1280, height: 850 } });
  const consoleErrors = [];
  page.on('console', message => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });

  await page.goto(gameUrl, { waitUntil: 'networkidle' });
  const canvas = page.locator('canvas');
  await canvas.waitFor({ state: 'visible' });
  await page.waitForTimeout(6000);
  await canvas.click({ position: { x: 640, y: 360 } });
  await page.keyboard.press('f');
  await page.waitForTimeout(500);
  await canvas.screenshot({ path: outputPath });
  await browser.close();

  console.log(JSON.stringify({ gameUrl, outputPath, consoleErrors }, null, 2));
  if (consoleErrors.length > 0) process.exitCode = 1;
})();
