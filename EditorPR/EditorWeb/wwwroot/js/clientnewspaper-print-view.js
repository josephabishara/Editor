// ════════════════════════════════════════════════════════════════════════
// VIEW AS HTML — reuses the exact visual language of Preview.cshtml
// (.paper / .media-card / .media-meta-grid / .mc-.ml-.mv / report-footer)
// so a manually-printed selection looks identical to a generated Report.
// ════════════════════════════════════════════════════════════════════════

function viewAsHtml() {
    const rows = getSelectedRows();
    if (!rows.length) { alert('Please select at least one item.'); return; }

    const win = window.open('', '_blank');
    if (!win) { alert('Popup blocked — please allow popups for this site.'); return; }

    const today = new Date().toLocaleDateString('en-GB', { day: '2-digit', month: 'long', year: 'numeric' });

    const cards = rows.map(d => buildMediaCard(d)).join('\n');

    win.document.open();
    win.document.write(PRINT_VIEW_TEMPLATE(clientName, today, cards));
    win.document.close();
}

function mc(label, value, full) {
    const val = (value === undefined || value === null || value === '') ? '—' : value;
    return `<div class="mc${full ? ' full' : ''}"><div class="ml">${label}</div><div class="mv">${val}</div></div>`;
}

function buildMediaCard(d) {
    const isRtl = isArabic(d.title || '');
    const titleHtml = d.articleurl
        ? `<a href="${d.articleurl}" target="_blank" rel="noopener">${d.title || ''}</a>`
        : (d.title || '');

    const contentTxt = decodeContent(d.content || '');
    const contentBlock = contentTxt
        ? `<div class="content-block"><div class="content-label">Content</div><div class="content-body">${contentTxt}</div></div>`
        : '';

    const imageBlock = d.screenshot
        ? `<div class="inline-image"><img src="${d.screenshot}" alt="${d.title || ''}" /></div>`
        : '';

    return `
    <div class="paper">
        <div class="section">
            <div class="section-heading">
                <span>${d.parentid ? 'Newspaper Item' : 'Media Item'}</span>
                <span class="section-sub">${d.date || ''}</span>
            </div>
            <div class="media-card">
                <div class="media-card-title ${isRtl ? 'rtl-title' : ''}">${titleHtml}</div>
                <div class="media-meta-grid">
                    ${mc('Date', d.date)}
                    ${mc('Category', d.category)}
                    ${mc('Media Tier', d.mediatier)}
                    ${mc('Publication / Website', d.website || d.publication)}
                    ${mc('Sub Category', d.subcategory)}
                    ${mc('Media Type', d.mediatype)}
                    ${mc('Frequency', d.frequency)}
                    ${mc('AD Value', d.ad)}
                    ${mc('Language', d.language)}
                    ${mc('Impression / Circulation', d.impression || d.circulation)}
                    ${mc('PR Value', d.pr)}
                    ${mc('Writer', d.writer)}
                    ${mc('Reach', d.reach)}
                    ${mc('Tone', d.toning)}
                    ${mc('Article Branding', d.articlebrand)}
                    ${mc('Headline Branding', d.headlinebrand)}
                    ${mc('Picture in Article', d.picture)}
                    ${mc('Generation', d.generation)}
                    ${mc('Parent ID', d.parentid || '—')}
                    ${mc('Created By', d.createdby)}
                    ${mc('Created At', d.createdat)}
                </div>
                ${contentBlock}
                ${imageBlock}
            </div>
        </div>
        <div class="report-footer">
            <span class="footer-brand"><strong>EditorPR</strong> — Media Monitoring &amp; PR Management</span>
            <span class="footer-logo"><span>${clientName}</span></span>
        </div>
        <div class="page-number"></div>
    </div>`;
}

function PRINT_VIEW_TEMPLATE(clientName, today, cardsHtml) {
    return `<!DOCTYPE html>
<html lang="en" dir="ltr">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width,initial-scale=1" />
<title>${clientName} — Media Selection</title>
<style>
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0}
*{-webkit-print-color-adjust:exact;print-color-adjust:exact;color-adjust:exact}
html{font-size:13px}
body{font-family:'Segoe UI',Arial,sans-serif;color:#1a1a1a;background:#e9e9ec}
.toolbar{background:#721a24;color:#fff;padding:10px 32px;display:flex;align-items:center;justify-content:space-between;gap:12px;position:sticky;top:0;z-index:100}
.toolbar-brand{font-size:12px;opacity:.7;letter-spacing:.5px;text-transform:uppercase}
.btn{padding:6px 16px;border-radius:5px;border:1px solid rgba(255,255,255,.25);font-size:12px;font-weight:500;cursor:pointer;background:rgba(255,255,255,.1);color:#fff}
.btn-print{background:#1a1a2e;border-color:#1a1a2e}
.page{max-width:900px;margin:32px auto}
.paper{background:#fff;box-shadow:0 1px 4px rgba(0,0,0,.15),0 0 0 1px rgba(0,0,0,.05);margin-bottom:28px;padding:36px 40px 90px;position:relative;min-height:1040px}
.section{margin-bottom:28px}
.section-heading{background:#721a24;color:#fff;padding:7px 14px;font-size:12px;font-weight:600;letter-spacing:.4px;text-transform:uppercase;display:flex;justify-content:space-between}
.section-heading .section-sub{font-size:11px;font-weight:400;opacity:.8;text-transform:none}
.media-card{border:1px solid #ddd;page-break-inside:avoid}
.media-card-title{background:#f7f7f7;border-bottom:1px solid #ddd;padding:10px 14px;font-size:13.5px;font-weight:700;line-height:1.45;color:#721a24}
.media-card-title.rtl-title{direction:rtl;text-align:center}
.media-card-title a{color:#721a24;text-decoration:none}
.media-meta-grid{display:grid;grid-template-columns:repeat(3,1fr)}
.mc{display:flex;border-right:1px solid #e8e8e8;border-bottom:1px solid #e8e8e8}
.mc:nth-child(3n){border-right:none}
.mc.full{grid-column:span 3;border-right:none}
.ml{background:#721a24;color:#fff;padding:6px 10px;font-size:11.5px;font-weight:500;min-width:120px;white-space:nowrap}
.mv{background:#fff;color:#721a24;padding:6px 10px;font-size:11.5px;flex:1}
.content-block{border-top:1px solid #ddd}
.content-block .content-label{background:#721a24;color:#fff;padding:6px 10px;font-size:11.5px;font-weight:600;text-transform:uppercase}
.content-block .content-body{padding:14px 16px;font-size:12.5px;line-height:1.8}
.inline-image{padding:14px 16px;border-top:1px solid #ddd}
.inline-image img{max-width:100%;max-height:360px;border:1px solid #ddd;display:block;margin:0 auto}
.report-footer{position:absolute;left:40px;right:40px;bottom:34px;border-top:2px solid #721a24;padding-top:14px;display:flex;justify-content:space-between;font-size:11px;color:#888}
.report-footer .footer-brand strong{color:#721a24}
.page-number{position:absolute;top:14px;right:24px;font-size:9px;color:#bbb}
@media print{
  @page{size:A4;margin:10mm}
  .toolbar{display:none!important}
  .page{max-width:none;margin:0}
  .paper{box-shadow:none;margin:0;page-break-after:always;break-after:page}
  .paper:last-child{page-break-after:auto}
  .media-card,.media-meta-grid,.content-block,.inline-image{page-break-inside:avoid;break-inside:avoid}
}
</style>
</head>
<body>
<div class="toolbar no-print">
    <span class="toolbar-brand">EditorPR — Media Selection — ${today}</span>
    <div><button class="btn btn-print" onclick="window.print()">🖨 Print / Save PDF</button>
    <button class="btn" onclick="window.close()">✕ Close</button></div>
</div>
<div class="page">${cardsHtml}</div>
</body>
</html>`;
}