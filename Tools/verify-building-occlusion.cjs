const fs = require('fs');
const { chromium } = require('playwright');

const gameUrl = process.argv[2] || 'http://127.0.0.1:8765/';
const outputPath = 'Builds/UI-QA/building-occlusion.png';

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
  await page.waitForTimeout(700);

  // Start to the left of the farm house, walk around its rear wall, then move
  // down toward the roof. The building sprite should cover the player here.
  await page.keyboard.down('w');
  await page.waitForTimeout(1000);
  await page.keyboard.up('w');
  await page.keyboard.down('d');
  await page.waitForTimeout(2200);
  await page.keyboard.up('d');
  await page.keyboard.down('s');
  await page.waitForTimeout(750);
  await page.keyboard.up('s');
  await page.waitForTimeout(700);

  await canvas.screenshot({ path: outputPath });
  await browser.close();

  console.log(JSON.stringify({ gameUrl, outputPath, consoleErrors }, null, 2));
  if (consoleErrors.length > 0) process.exitCode = 1;
})();
