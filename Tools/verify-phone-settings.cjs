const fs = require('fs');
const { chromium } = require('playwright');

const gameUrl = process.argv[2] || 'http://127.0.0.1:8765/';
const outputDirectory = 'Builds/UI-QA';

(async () => {
  fs.mkdirSync(outputDirectory, { recursive: true });
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1280, height: 850 } });
  const consoleErrors = [];
  const failedResponses = [];
  page.on('console', message => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });
  page.on('response', response => {
    if (response.status() >= 400) {
      failedResponses.push({ status: response.status(), url: response.url() });
    }
  });

  await page.goto(gameUrl, { waitUntil: 'networkidle' });
  const canvas = page.locator('canvas');
  await canvas.waitFor({ state: 'visible' });
  await page.waitForTimeout(6000);
  await canvas.click({ position: { x: 640, y: 360 } });
  await page.keyboard.press('f');
  await page.waitForTimeout(1000);

  await page.keyboard.press('p');
  await page.waitForTimeout(500);
  await canvas.click({ position: { x: 525, y: 189 } });
  await page.waitForTimeout(400);
  await canvas.screenshot({ path: `${outputDirectory}/phone-settings.png` });

  await canvas.click({ position: { x: 425, y: 372 } });
  await page.waitForTimeout(300);
  await canvas.screenshot({ path: `${outputDirectory}/phone-settings-muted.png` });

  await canvas.click({ position: { x: 425, y: 499 } });
  await page.waitForTimeout(300);
  await canvas.screenshot({ path: `${outputDirectory}/phone-exit-confirmation.png` });

  await canvas.click({ position: { x: 520, y: 422 } });
  await page.waitForTimeout(500);
  await canvas.screenshot({ path: `${outputDirectory}/phone-exit-saved.png` });

  await browser.close();
  console.log(JSON.stringify({ gameUrl, outputDirectory, consoleErrors, failedResponses }, null, 2));
  if (consoleErrors.length > 0 && failedResponses.length === 0) process.exitCode = 1;
})();
