function escapeHtml(str) {
    return String(str ?? '').replace(/[&<>"']/g, c => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[c]));
}

function viewAsHtml() {
    const rows = getSelectedRows();
    if (!rows.length) { alert('Please select at least one article.'); return; }

    const pages = rows.map((d, idx) => {
        const title = escapeHtml(decodeContent(d.title || 'Untitled'));
        const content = escapeHtml(decodeContent(d.content || ''));
        const rtlTitle = isArabic(d.title || '') ? 'dir="rtl"' : '';
        const rtlContent = isArabic(d.content || '') ? 'dir="rtl"' : '';

        const gridDef = [
            ['Website', d.website, 'Date', d.date, 'Ad Value', d.ad],
            ['Category', d.category, 'Sub Category', d.subcategory, 'PR Value', d.pr],
            ['Media Type', d.mediatype, 'Media Tier', d.mediatier, 'Impression', d.impression],
            ['Frequency', d.frequency, 'Article URL', d.url, 'Reach', d.reach],
            ['Language', d.language, 'Writer', d.writer, 'Tone', d.toning],
            ['Picture In Article', d.picture, 'Headline Branding', d.headlinebrand, 'Generation', d.generation],
            ['Article Branding', d.articlebrand, '', '', '', '']
        ];

        const gridHtml = gridDef.map(r => `
            <div class="grid-row">
                <div class="cell"><span class="lbl">${escapeHtml(r[0])}</span><span class="val">${escapeHtml(r[1] || '—')}</span></div>
                <div class="cell"><span class="lbl">${escapeHtml(r[2])}</span><span class="val">${escapeHtml(r[3] || '—')}</span></div>
                <div class="cell"><span class="lbl">${escapeHtml(r[4])}</span><span class="val">${escapeHtml(r[5] || '—')}</span></div>
            </div>`).join('');

        return `
        <section class="print-page">
            <div class="top-row">${escapeHtml(clientName)}</div>
            <h2 class="title" ${rtlTitle}>${title}</h2>
            <hr/>
            <div class="grid">${gridHtml}</div>
            <h3 class="title-lg" ${rtlTitle}>${title}</h3>
            ${d.screenshot ? `<div class="img-wrap"><img src="${escapeHtml(d.screenshot)}" /></div>` : ''}
            <div class="content" ${rtlContent}>${content}</div>
            <div class="footer">
                <div class="f-left"> ${escapeHtml(clientName)}   <br><small>  ${rtlTitle} </small></div>
                <div class="f-page">${idx + 1} / ${rows.length}</div>
                <div class="f-right">EDITOR<br><small>PR &amp; COMMUNICATIONS</small></div>
            </div>
        </section>`;
    }).join('');

    const html = `<!DOCTYPE html>
<html lang="ar">
<head>
<meta charset="utf-8" />
<title>${escapeHtml(clientName)} — Articles Report</title>
<style>
  @page { size: A4; margin: 14mm; }
  * { box-sizing: border-box; }
  body { font-family: 'Segoe UI', Tahoma, Arial, sans-serif; color:#1e1e1e; margin:0; text-align:center; padding-left : 150px; padding-right : 150px;}
  .toolbar { position: sticky; top:0; background:#0f172a; padding:.6rem 1rem; display:flex; gap:.6rem; }
  .toolbar button { border:none; border-radius:6px; padding:.45rem 1rem; font-weight:600; cursor:pointer; }
  .btn-print { background:#16a34a; color:#fff; }
  .btn-close { background:#475569; color:#fff; }
  .print-page { padding: 10mm 0; page-break-after: always; }
  .print-page:last-child { page-break-after: auto; }
  .top-row { text-align:left; font-size:.8rem; color:#5a6a7a; margin-bottom:6px; }
  .title { text-align:left; font-size:1.1rem; margin:0 0 8px; }
  hr { border:none; border-top:1px solid #c8c8c8; margin-bottom:10px; }
  .grid { border:1px solid #c8c8c8; }
  .grid-row { display:grid; grid-template-columns: 1fr 1fr 1fr; }
  .grid-row:nth-child(even) .val { background:#f5f5f5; }
  .cell { display:flex; border-right:1px solid #c8c8c8; }
  .cell:last-child { border-right:none; }
  .lbl { background:#8b0000; color:#fff; font-size:.65rem; font-weight:700; padding:.35rem; min-width:80px; display:flex; align-items:center; }
  .val { padding:.35rem; font-size:.72rem; flex:1; display:flex; align-items:center; }
  .title-lg { text-align:left; font-size:1.4rem; font-weight:700; margin:18px 0 10px; }
  .img-wrap { text-align:center; margin-bottom:14px; }
  .img-wrap img { max-width:90mm; max-height:60mm; border:1px solid #ddd; }
  .content { font-size:.85rem; line-height:1.5; column-count:2; column-gap:6mm; text-align:justify; }
  .footer { display:flex; justify-content:space-between; align-items:center; border-top:1px solid #c8c8c8; margin-top:16px; padding-top:8px; font-size:.7rem; color:#5a6a7a; }
  .f-left, .f-right { color:#8b0000; font-weight:700; }
  @media print { .toolbar { display:none; } }
</style>
</head>
<body>
  <div class="toolbar">
    <button class="btn-print" onclick="window.print()">🖨 Print</button>
    <button class="btn-close" onclick="window.close()">✕ Close</button>
  </div>
  ${pages}
</body>
</html>`;

    const win = window.open('', '_blank');
    if (!win) { alert('Please allow pop-ups to view the report.'); return; }
    win.document.open();
    win.document.write(html);
    win.document.close();
}