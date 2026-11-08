const fs = require('fs');
const { chromium } = require('playwright');

const gameUrl = process.argv[2] || 'http://127.0.0.1:8765/';
const outputDirectory = 'Builds/UI-QA/diagonal-facing';

async function captureDiagonal(page, canvas, horizontalKey, outputName) {
  await page.keyboard.down('w');
  await page.keyboard.down(horizontalKey);
  await page.waitForTimeout(700);
  await canvas.screenshot({ path: `${outputDirectory}/${outputName}.png` });
  await page.keyboard.up(horizontalKey);
  await page.keyboard.up('w');
  await page.waitForTimeout(300);
}

(async () => {
  fs.mkdirSync(outputDirectory, { recursive: true });
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

  await captureDiagonal(page, canvas, 'd', 'up-right');
  await captureDiagonal(page, canvas, 'a', 'up-left');

  await browser.close();
  console.log(JSON.stringify({ gameUrl, outputDirectory, consoleErrors }, null, 2));
  if (consoleErrors.length > 0) process.exitCode = 1;
})();
