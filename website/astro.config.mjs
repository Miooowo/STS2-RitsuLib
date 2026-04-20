// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// GitHub Pages 项目页：https://miooowo.github.io/STS2-RitsuLib/
const site = 'https://miooowo.github.io';
const base = '/STS2-RitsuLib';

// https://astro.build/config
export default defineConfig({
	site,
	base,
	integrations: [
		starlight({
			title: 'STS2 RitsuLib',
			description: 'STS2 Mod 开发框架 RitsuLib 中文文档',
			defaultLocale: 'root',
			locales: {
				root: {
					label: '简体中文',
					lang: 'zh-CN',
				},
			},
			social: [
				{
					icon: 'github',
					label: 'GitHub',
					href: 'https://github.com/Miooowo/STS2-RitsuLib',
				},
			],
			sidebar: [
				{
					label: '总览',
					items: [{ label: '首页', link: '/' }],
				},
				{
					label: '入门与架构',
					items: [
						{ label: '快速入门', slug: 'getting-started' },
						{ label: '框架设计', slug: 'framework-design' },
						{ label: '术语表', slug: 'terminology' },
						{ label: '诊断与兼容层', slug: 'diagnostics-and-compatibility' },
					],
				},
				{
					label: '内容与注册',
					items: [
						{ label: '内容包与注册器', slug: 'content-packs-and-registries' },
						{ label: '内容注册规则', slug: 'content-authoring-toolkit' },
						{ label: '角色与解锁模板', slug: 'character-and-unlock-scaffolding' },
						{ label: '时间线与解锁', slug: 'timeline-and-unlocks' },
						{ label: '自定义事件', slug: 'custom-events' },
					],
				},
				{
					label: '卡牌与展示',
					items: [
						{ label: '卡牌动态变量', slug: 'card-dynamic-var-toolkit' },
						{ label: 'LocString 占位符解析', slug: 'loc-string-placeholder-resolution' },
						{ label: '本地化与关键词', slug: 'localization-and-keywords' },
					],
				},
				{
					label: '运行时与扩展',
					items: [
						{ label: '生命周期事件', slug: 'lifecycle-events' },
						{ label: '持久化设计', slug: 'persistence-guide' },
						{ label: '补丁系统', slug: 'patching-guide' },
						{ label: 'Mod 设置界面', slug: 'mod-settings' },
					],
				},
				{
					label: '资源与场景',
					items: [
						{ label: '资源配置与回退规则', slug: 'asset-profiles-and-fallbacks' },
						{ label: 'Godot 场景编写说明', slug: 'godot-scene-authoring' },
						{ label: 'FMOD 与音频', slug: 'fmod-and-audio' },
					],
				},
			],
			lastUpdated: true,
		}),
	],
});
