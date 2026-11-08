const fs = require('fs');
const { chromium } = require('playwright');

const gameUrl = process.argv[2] || 'http://127.0.0.1:8765/';
const outputDirectory = 'Builds/UI-QA';

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
  await page.waitForTimeout(1000);

  await page.keyboard.press('p');
  await page.waitForTimeout(800);
  await canvas.screenshot({ path: `${outputDirectory}/quests-phone.png` });
  await page.keyboard.press('p');

  await page.keyboard.press('i');
  await page.waitForTimeout(800);
  await canvas.screenshot({ path: `${outputDirectory}/inventory.png` });
  await page.keyboard.press('i');

  await page.keyboard.down('a');
  await page.waitForTimeout(3200);
  await page.keyboard.up('a');
  await page.keyboard.down('w');
  await page.waitForTimeout(350);
  await page.keyboard.up('w');
  await page.waitForTimeout(500);
  await canvas.screenshot({ path: `${outputDirectory}/farm-props.png` });

  await page.keyboard.down('w');
  await page.waitForTimeout(5200);
  await page.keyboard.up('w');
  await page.waitForTimeout(12000);
  await canvas.screenshot({ path: `${outputDirectory}/npc-social.png` });

  await browser.close();
  console.log(JSON.stringify({ gameUrl, outputDirectory, consoleErrors }, null, 2));
  if (consoleErrors.length > 0) process.exitCode = 1;
})();
