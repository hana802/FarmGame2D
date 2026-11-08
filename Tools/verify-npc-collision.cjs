const fs = require('fs');
const { chromium } = require('playwright');

const gameUrl = process.argv[2] || 'http://127.0.0.1:8765/';
const outputDirectory = 'Builds/UI-QA/npc-collision';

async function hold(page, key, milliseconds) {
  await page.keyboard.down(key);
  await page.waitForTimeout(milliseconds);
  await page.keyboard.up(key);
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

  // Walk north from the farm, detouring around the tree on the center line.
  await hold(page, 'w', 4700);
  await hold(page, 'd', 1300);
  await hold(page, 'w', 2500);
  await hold(page, 'a', 1300);
  await page.waitForTimeout(1000);

  for (let index = 1; index <= 3; index += 1) {
    await canvas.screenshot({ path: `${outputDirectory}/village-${index}.png` });
    await page.waitForTimeout(5000);
  }

  await browser.close();
  console.log(JSON.stringify({ gameUrl, outputDirectory, consoleErrors }, null, 2));
  if (consoleErrors.length > 0) process.exitCode = 1;
})();
