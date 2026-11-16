const fs = require('fs');
const { chromium } = require('playwright');

const gameUrl = process.argv[2] || 'http://127.0.0.1:8765/';
const outputDirectory = 'Builds/UI-QA';

(async () => {
  fs.mkdirSync(outputDirectory, { recursive: true });
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1280, height: 850 } });
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto(gameUrl, { waitUntil: 'networkidle' });
  const canvas = page.locator('#unity-canvas');
  await canvas.waitFor({ state: 'visible' });
  await page.waitForTimeout(6000);
  await canvas.click({ position: { x: 640, y: 360 } });
  await page.keyboard.press('f');
  await page.waitForTimeout(700);
  await page.keyboard.press('i');
  await page.waitForTimeout(400);
  await canvas.click({ position: { x: 700, y: 220 } });
  await page.waitForTimeout(400);
  await canvas.screenshot({ path: `${outputDirectory}/town-decor-inventory.png` });
  await browser.close();
  console.log(JSON.stringify({ errors, screenshot: `${outputDirectory}/town-decor-inventory.png` }));
  if (errors.length > 0) process.exitCode = 1;
})();
