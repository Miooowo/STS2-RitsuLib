/**
 * 从仓库根目录的 Docs/zh 同步 Markdown，转为 Starlight 友好格式。
 */
import fs from 'node:fs/promises';
import path from 'node:path';

const root = path.resolve(import.meta.dirname, '..');
const srcDir = path.join(root, '..', 'Docs', 'zh');
const outDir = path.join(root, 'src', 'content', 'docs');

function pascalStemToKebab(stem) {
	return stem
		.replace(/([a-z\d])([A-Z])/g, '$1-$2')
		.replace(/([A-Z]+)([A-Z][a-z])/g, '$1-$2')
		.toLowerCase();
}

const mdFiles = (await fs.readdir(srcDir)).filter((f) => f.endsWith('.md'));
const stemToSlug = new Map();
for (const file of mdFiles) {
	const stem = path.basename(file, '.md');
	stemToSlug.set(stem, pascalStemToKebab(stem));
}

function extractTitleAndStripH1(body) {
	const m = body.match(/^#\s+(.+)\r?\n\r?\n?/);
	if (!m) return { title: '文档', body };
	const title = m[1].trim();
	const rest = body.slice(m[0].length);
	return { title, body: rest };
}

function rewriteLinks(body) {
	return body.replace(/\(([^)\s]+\.md)\)/g, (full, ref) => {
		const stem = path.basename(ref, '.md');
		const slug = stemToSlug.get(stem);
		if (!slug) return full;
		return `(/${slug}/)`;
	});
}

for (const file of mdFiles) {
	const stem = path.basename(file, '.md');
	const slug = stemToSlug.get(stem);
	let body = await fs.readFile(path.join(srcDir, file), 'utf8');
	const parsed = extractTitleAndStripH1(body);
	const title = parsed.title;
	body = rewriteLinks(parsed.body);
	const out = `---
title: ${JSON.stringify(title)}
---

${body.trimStart()}\n`;
	const outName = `${slug}.md`;
	await fs.writeFile(path.join(outDir, outName), out, 'utf8');
}

console.log(`Synced ${mdFiles.length} files → ${outDir}`);
