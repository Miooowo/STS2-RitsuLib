// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// GitHub Pages 项目页：https://miooowo.github.io/STS2-RitsuLib/
const site = 'https://miooowo.github.io';
const base = '/STS2-RitsuLib';

/** @param {string} zh @param {string} en */
const lb = (zh, en) => ({ label: zh, translations: { en } });
/** @param {string} zh @param {string} en */
const group = (zh, en) => ({ label: zh, translations: { en } });

// https://astro.build/config
export default defineConfig({
	site,
	base,
	integrations: [
		starlight({
			title: 'STS2 RitsuLib',
			description:
				'STS2 Mod 开发框架 RitsuLib 文档（简体中文默认，English in /en/） | STS2 RitsuLib modding framework docs (default zh-CN, English under /en/).',
			defaultLocale: 'root',
			locales: {
				root: {
					label: '简体中文',
					lang: 'zh-CN',
				},
				en: {
					label: 'English',
					lang: 'en',
				},
			},
			social: [
				{
					icon: 'github',
					label: 'GitHub',
					href: 'https://github.com/Miooowo/STS2-RitsuLib',
				},
			],
			// 静态部署（GitHub Pages）无服务端中间件：在中文首页用客户端根据语言偏好跳转 /en/（与 cookie `ritsulib-locale` 协同）
			head: [
				{
					tag: 'script',
					attrs: {},
					content: `(() => {
	var B = ${JSON.stringify(base + '/')};
	var PC = B.endsWith('/') ? B.slice(0, -1) : B;
	function pathNorm() {
		var p = location.pathname;
		return p.endsWith('/') && p.length > 1 ? p.slice(0, -1) : p;
	}
	function isZhHome() {
		var p = pathNorm();
		return p === PC || p === PC + '/index.html';
	}
	function isEnHome() {
		var p = pathNorm();
		return p === PC + '/en' || p === PC + '/en/index.html';
	}
	function getCookie(name) {
		var m = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
		return m ? decodeURIComponent(m[1]) : '';
	}
	function setCookie(name, val) {
		document.cookie =
			name + '=' + encodeURIComponent(val) + ';path=/;max-age=31536000;samesite=lax';
	}
	if (isEnHome()) {
		setCookie('ritsulib-locale', 'en');
		return;
	}
	if (!isZhHome()) return;
	var c = getCookie('ritsulib-locale');
	if (c === 'en') {
		setCookie('ritsulib-locale', 'zh');
		return;
	}
	if (c === 'zh') return;
	var langs = (navigator.languages || [navigator.language || '']).map(function (s) {
		return String(s).toLowerCase().split('-')[0];
	});
	function prefersEn() {
		for (var i = 0; i < langs.length; i++) {
			if (langs[i] === 'zh') return false;
			if (langs[i] === 'en') return true;
		}
		return true;
	}
	if (prefersEn()) {
		setCookie('ritsulib-locale', 'en');
		location.replace(PC + '/en/');
	} else setCookie('ritsulib-locale', 'zh');
})();`,
				},
			],
			sidebar: [
				{
					...group('总览', 'Overview'),
					items: [{ ...lb('首页', 'Home'), link: '/' }],
				},
				{
					...group('入门与架构', 'Intro & architecture'),
					items: [
						{ ...lb('快速入门', 'Getting started'), slug: 'getting-started' },
						{ ...lb('框架设计', 'Framework design'), slug: 'framework-design' },
						{ ...lb('术语表', 'Terminology'), slug: 'terminology' },
						{ ...lb('诊断与兼容层', 'Diagnostics & compatibility'), slug: 'diagnostics-and-compatibility' },
					],
				},
				{
					...group('内容与注册', 'Content & registration'),
					items: [
						{ ...lb('内容包与注册器', 'Content packs & registries'), slug: 'content-packs-and-registries' },
						{ ...lb('内容注册规则', 'Content authoring rules'), slug: 'content-authoring-toolkit' },
						{ ...lb('角色与解锁模板', 'Character & unlock scaffolding'), slug: 'character-and-unlock-scaffolding' },
						{ ...lb('时间线与解锁', 'Timeline & unlocks'), slug: 'timeline-and-unlocks' },
						{ ...lb('自定义事件', 'Custom events'), slug: 'custom-events' },
					],
				},
				{
					...group('卡牌与展示', 'Cards & presentation'),
					items: [
						{ ...lb('卡牌动态变量', 'Card dynamic variables'), slug: 'card-dynamic-var-toolkit' },
						{ ...lb('LocString 占位符解析', 'LocString placeholders'), slug: 'loc-string-placeholder-resolution' },
						{ ...lb('本地化与关键词', 'Localization & keywords'), slug: 'localization-and-keywords' },
					],
				},
				{
					...group('运行时与扩展', 'Runtime & extensions'),
					items: [
						{ ...lb('生命周期事件', 'Lifecycle events'), slug: 'lifecycle-events' },
						{ ...lb('持久化设计', 'Persistence'), slug: 'persistence-guide' },
						{ ...lb('补丁系统', 'Patching'), slug: 'patching-guide' },
						{ ...lb('Mod 设置界面', 'Mod settings UI'), slug: 'mod-settings' },
					],
				},
				{
					...group('资源与场景', 'Assets & scenes'),
					items: [
						{ ...lb('资源配置与回退规则', 'Asset profiles & fallbacks'), slug: 'asset-profiles-and-fallbacks' },
						{ ...lb('生物视觉与动画', 'Creature visuals & animation'), slug: 'creature-visuals-and-animation' },
						{ ...lb('Godot 场景编写说明', 'Godot scene authoring'), slug: 'godot-scene-authoring' },
						{ ...lb('FMOD 与音频', 'FMOD & audio'), slug: 'fmod-and-audio' },
					],
				},
			],
			lastUpdated: true,
		}),
	],
});
