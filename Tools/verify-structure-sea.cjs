const fs = require('fs');
const { chromium } = require('playwright');

const gameUrl = process.argv[2] || 'http://127.0.0.1:8765/';
const outputDirectory = 'Builds/UI-QA/structure-sea';

async function hold(page, key, milliseconds) {
  await page.keyboard.down(key);
  await page.waitForTimeout(milliseconds);
  await page.keyboard.up(key);
}

async function openGame(browser, suffix, consoleErrors) {
  const context = await browser.newContext({ viewport: { width: 1280, height: 850 } });
  const page = await context.newPage();
  page.on('console', message => {
    if (message.type() === 'error') consoleErrors.push(`${suffix}: ${message.text()}`);
  });
  await page.goto(`${gameUrl}${gameUrl.includes('?') ? '&' : '?'}qa=${suffix}`, { waitUntil: 'networkidle' });
  const canvas = page.locator('canvas');
  await canvas.waitFor({ state: 'visible' });
  await page.waitForTimeout(6000);
  await canvas.click({ position: { x: 640, y: 360 } });
  await page.keyboard.press('f');
  await page.waitForTimeout(500);
  return { context, page, canvas };
}

(async () => {
  fs.mkdirSync(outputDirectory, { recursive: true });
  const browser = await chromium.launch({ headless: true });
  const consoleErrors = [];

  const coast = await openGame(browser, 'coast', consoleErrors);
  await hold(coast.page, 's', 2600);
  await coast.page.waitForTimeout(600);
  await coast.canvas.screenshot({ path: `${outputDirectory}/south-coast.png` });
  await coast.context.close();

  const square = await openGame(browser, 'square', consoleErrors);
  await hold(square.page, 'w', 4700);
  await hold(square.page, 'd', 1600);
  await hold(square.page, 'w', 1800);
  await hold(square.page, 'a', 1600);
  await hold(square.page, 's', 450);
  await square.page.waitForTimeout(600);
  await square.canvas.screenshot({ path: `${outputDirectory}/village-square-occlusion.png` });
  await square.context.close();

  await browser.close();
  console.log(JSON.stringify({ gameUrl, outputDirectory, consoleErrors }, null, 2));
  if (consoleErrors.length > 0) process.exitCode = 1;
})();
