/**
 * 从仓库根目录的 Docs/zh、Docs/en 同步 Markdown，转为 Starlight 友好格式。
 * 站内互链使用相对路径：各文档页在 /.../slug/ 一层目录下，互链须用 ../目标/；
 * 仅各语言首页 index 使用 ./目标/（与 sibling 同级）。
 */
import fs from 'node:fs/promises';
import path from 'node:path';

const root = path.resolve(import.meta.dirname, '..');
const zhDir = path.join(root, '..', 'Docs', 'zh');
const enDir = path.join(root, '..', 'Docs', 'en');
const docsOut = path.join(root, 'src', 'content', 'docs');
const enOut = path.join(docsOut, 'en');

function pascalStemToKebab(stem) {
	return stem
		.replace(/([a-z\d])([A-Z])/g, '$1-$2')
		.replace(/([A-Z]+)([A-Z][a-z])/g, '$1-$2')
		.toLowerCase();
}

async function listMd(dir) {
	try {
		return (await fs.readdir(dir)).filter((f) => f.endsWith('.md'));
	} catch {
		return [];
	}
}

const zhFiles = await listMd(zhDir);
const enFiles = await listMd(enDir);
const stems = new Set();
for (const f of zhFiles) stems.add(path.basename(f, '.md'));
for (const f of enFiles) stems.add(path.basename(f, '.md'));

const stemToSlug = new Map();
for (const stem of stems) {
	stemToSlug.set(stem, pascalStemToKebab(stem));
}

function extractTitleAndStripH1(body) {
	const m = body.match(/^#\s+(.+)\r?\n\r?\n?/);
	if (!m) return { title: '文档', body };
	const title = m[1].trim();
	const rest = body.slice(m[0].length);
	return { title, body: rest };
}

/** @param {string} body @param {string} pageSlug kebab slug of the page file (e.g. index, getting-started) */
function rewriteLinks(body, pageSlug) {
	const rel = pageSlug === 'index' ? './' : '../';
	return body.replace(/\(([^)\s]+\.md)\)/g, (full, ref) => {
		const stem = path.basename(ref, '.md');
		const slug = stemToSlug.get(stem);
		if (!slug) return full;
		return `(${rel}${slug}/)`;
	});
}

/** @param {string} srcFile @param {string} destFile @param {string} pageSlug */
async function syncFile(srcFile, destFile, pageSlug) {
	let body = await fs.readFile(srcFile, 'utf8');
	const parsed = extractTitleAndStripH1(body);
	const title = parsed.title;
	body = rewriteLinks(parsed.body, pageSlug);
	const out = `---
title: ${JSON.stringify(title)}
---

${body.trimStart()}\n`;
	await fs.writeFile(destFile, out, 'utf8');
}

await fs.mkdir(enOut, { recursive: true });

for (const file of zhFiles) {
	const stem = path.basename(file, '.md');
	const slug = stemToSlug.get(stem);
	const destName = `${slug}.md`;
	await syncFile(path.join(zhDir, file), path.join(docsOut, destName), slug);
}

for (const file of enFiles) {
	const stem = path.basename(file, '.md');
	const slug = stemToSlug.get(stem);
	const destName = `${slug}.md`;
	await syncFile(path.join(enDir, file), path.join(enOut, destName), slug);
}

console.log(`Synced ${zhFiles.length} zh → ${docsOut}`);
console.log(`Synced ${enFiles.length} en → ${enOut}`);
