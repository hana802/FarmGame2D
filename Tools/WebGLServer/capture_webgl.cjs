const { chromium } = require('playwright');
const path = require('path');

async function hold(page, key, milliseconds) {
  await page.keyboard.down(key);
  await page.waitForTimeout(milliseconds);
  await page.keyboard.up(key);
  await page.waitForTimeout(250);
}

(async () => {
  const outputDirectory = path.resolve(process.argv[2] || 'Builds/WebGL');
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
  const errors = [];
  page.on('console', message => {
    if (message.type() === 'error') errors.push(`console: ${message.text()}`);
  });
  page.on('pageerror', error => errors.push(`page: ${error.message}`));

  await page.goto('http://127.0.0.1:8765/', { waitUntil: 'networkidle', timeout: 120000 });
  const canvas = page.locator('#unity-canvas');
  await canvas.waitFor({ state: 'visible', timeout: 120000 });
  await page.waitForTimeout(12000);
  await canvas.click({ position: { x: 640, y: 360 } });
  await page.keyboard.press('KeyM');
  await page.waitForTimeout(1500);
  await page.keyboard.press('Digit1');
  await page.waitForTimeout(300);
  await page.screenshot({ path: path.join(outputDirectory, 'review-held-hoe.png'), fullPage: true });
  await page.keyboard.press('Digit2');
  await page.waitForTimeout(300);
  await page.screenshot({ path: path.join(outputDirectory, 'review-held-seeds.png'), fullPage: true });
  await page.keyboard.press('Digit3');
  await page.waitForTimeout(300);
  await page.screenshot({ path: path.join(outputDirectory, 'review-held-watering-can.png'), fullPage: true });
  await hold(page, 'ArrowDown', 100);
  await page.screenshot({ path: path.join(outputDirectory, 'review-farm.png'), fullPage: true });

  await hold(page, 'ArrowRight', 1800);
  await page.keyboard.press('KeyE');
  await page.waitForTimeout(1500);
  await page.screenshot({ path: path.join(outputDirectory, 'review-interior.png'), fullPage: true });

  await page.keyboard.press('KeyE');
  await page.waitForTimeout(500);
  await hold(page, 'ArrowLeft', 2050);
  await hold(page, 'ArrowUp', 5650);
  await hold(page, 'ArrowRight', 900);
  await page.screenshot({ path: path.join(outputDirectory, 'review-village-art.png'), fullPage: true });
  await page.keyboard.press('KeyE');
  await page.waitForTimeout(700);
  await page.screenshot({ path: path.join(outputDirectory, 'review-dialogue.png'), fullPage: true });

  console.log(JSON.stringify({ screenshots: 7, errors }, null, 2));
  await browser.close();
})();
