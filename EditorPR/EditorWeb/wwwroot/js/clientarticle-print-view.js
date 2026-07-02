// ─────────────────────────────────────────────────────────────────────────────
// clientarticle-print-view.js
// Matches Preview.cshtml exactly:
//   · maroon (#721a24) meta-grid with .ml / .mv cells (3-col)
//   · .paper "page" wrapper with pinned footer
//   · sticky toolbar with Print / Close buttons
//   · content block + inline first image + extra images on own pages
//   · print-color-adjust: exact so maroon headers survive printing
// ─────────────────────────────────────────────────────────────────────────────

const PREVIEW_CSS = `
  *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
  * { -webkit-print-color-adjust:exact; print-color-adjust:exact; color-adjust:exact; }
  html { font-size:13px; }
  body { font-family:'Segoe UI',Arial,sans-serif; color:#1a1a1a; background:#e9e9ec; padding:0; }

  .toolbar { background:#721a24; color:#fff; padding:10px 32px; display:flex;
    align-items:center; justify-content:space-between; gap:12px;
    position:sticky; top:0; z-index:100; }
  .toolbar-brand { font-size:12px; opacity:.7; letter-spacing:.5px; text-transform:uppercase; }
  .toolbar-actions { display:flex; gap:8px; }
  .btn { padding:6px 16px; border-radius:5px; border:1px solid rgba(255,255,255,.25);
    font-size:12px; font-weight:500; cursor:pointer; background:rgba(255,255,255,.1);
    color:#fff; display:inline-flex; align-items:center; gap:6px; text-decoration:none; }
  .btn-print { background:#1a1a2e; border-color:#1a1a2e; }

  .page { max-width:900px; margin:32px auto; padding:0; }
  .paper { background:#fff; box-shadow:0 1px 4px rgba(0,0,0,.15),0 0 0 1px rgba(0,0,0,.05);
    margin-bottom:28px; padding:36px 40px 90px; position:relative; min-height:1040px; }
  .paper .page-number { position:absolute; top:14px; right:24px; font-size:9px; color:#bbb; }

  .report-title-block { border-bottom:2px solid #721a24; padding-bottom:14px; margin-bottom:20px; }
  .report-title-block h1 { font-size:26px; font-weight:700; color:#721a24; margin-bottom:8px; }
  .report-meta-row { display:flex; align-items:center; gap:12px; flex-wrap:wrap; font-size:11.5px; color:#555; }
  .report-meta-row strong { color:#721a24; }

  .section { margin-bottom:28px; }
  .section-heading { background:#721a24; color:#fff; padding:7px 14px; font-size:12px;
    font-weight:600; letter-spacing:.4px; text-transform:uppercase;
    display:flex; justify-content:space-between; align-items:center; }
  .section-heading .section-sub { font-size:11px; font-weight:400; opacity:.8;
    text-transform:none; letter-spacing:0; }

  .media-card { border:1px solid #ddd; margin-bottom:20px; page-break-inside:avoid; }
  .media-card-title { background:#f7f7f7; border-bottom:1px solid #ddd; padding:10px 14px;
    font-size:13.5px; font-weight:700; line-height:1.45; color:#721a24; }
  .media-card-title.rtl-title { direction:rtl; text-align:center; }
  .media-card-title a { color:#721a24; text-decoration:none; }
  .media-card-title a:hover { color:#c0392b; text-decoration:underline; }

  .media-meta-grid { display:grid; grid-template-columns:repeat(3,1fr); }
  .mc { display:flex; border-right:1px solid #e8e8e8; border-bottom:1px solid #e8e8e8; }
  .mc:nth-child(3n) { border-right:none; }
  .mc.full { grid-column:span 3; border-right:none; }
  .ml { background:#721a24; color:#fff; padding:6px 10px; font-size:11.5px;
    font-weight:500; min-width:120px; white-space:nowrap; }
  .mv { background:#fff; color:#721a24; padding:6px 10px; font-size:11.5px; flex:1; }
  .mv a { color:#c0392b; text-decoration:none; word-break:break-all; }

  .content-block { border-top:1px solid #ddd; }
  .content-block .content-label { background:#721a24; color:#fff; padding:6px 10px;
    font-size:11.5px; font-weight:600; text-transform:uppercase; letter-spacing:.4px; }
  .content-block .content-body { padding:14px 16px; font-size:12.5px; line-height:1.8; color:#1a1a1a; }
  .content-body p { margin-bottom:8px; }
  .content-body img { max-width:100%; }

  .inline-image { padding:14px 16px; border-top:1px solid #ddd; }
  .inline-image img { max-width:100%; max-height:360px; border:1px solid #ddd; display:block; margin:0 auto; }
  .image-page { display:flex; flex-direction:column; align-items:center;
    justify-content:center; height:100%; padding:24px; }
  .image-page .image-page-label { font-size:11px; color:#888; text-transform:uppercase;
    letter-spacing:1px; margin-bottom:14px; }
  .image-page img { max-width:100%; max-height:780px; border:1px solid #ddd; object-fit:contain; }

  .report-footer { position:absolute; left:40px; right:40px; bottom:34px;
    border-top:2px solid #721a24; padding-top:14px;
    display:flex; justify-content:space-between; align-items:center;
    font-size:11px; color:#888; }
  .report-footer .footer-brand strong { color:#721a24; }
  .report-footer .footer-logo { display:flex; align-items:center; gap:8px; }

  @media print {
    @page { size:A4; margin:10mm; }
    html, body { background:#fff; width:100%; }
    .toolbar { display:none !important; }
    .page { max-width:none; margin:0; padding:0; }
    .paper { box-shadow:none; margin:0; height:277mm; min-height:0; max-height:277mm;
      width:100%; padding-bottom:26mm; overflow:hidden;
      page-break-after:always; break-after:page; }
    .paper:last-child { page-break-after:auto; break-after:auto; }
    .paper .page-number { display:none; }
    .media-card,.media-meta-grid,.content-block,.inline-image,.image-page
      { page-break-inside:avoid; break-inside:avoid; }
    .section-heading { page-break-after:avoid; break-after:avoid; }
    .report-footer { position:absolute; bottom:8mm; left:12mm; right:12mm; }
  }
`;

// ── Helpers ───────────────────────────────────────────────────────────────────
function escHtml(s) {
    return String(s ?? '').replace(/[&<>"']/g, c =>
        ({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;' }[c])
    );
}

function isRtl(str) {
    return /[\u0600-\u06FF\u0750-\u077F\uFB50-\uFDFF\uFE70-\uFEFF]/.test(str || '');
}

function mc(label, val) {
    return `<div class="mc"><div class="ml">${escHtml(label)}</div><div class="mv">${escHtml(val||'—')}</div></div>`;
}

function mcLink(label, url) {
    const inner = url
        ? `<a href="${escHtml(url)}" target="_blank" rel="noopener">Article URL</a>`
        : '—';
    return `<div class="mc"><div class="ml">${escHtml(label)}</div><div class="mv">${inner}</div></div>`;
}

function footer(clientName, reportDate) {
    return `
    <div class="report-footer">
        <span class="footer-brand"><strong>EditorPR</strong> — Media Monitoring &amp; PR Management</span>
        <span class="footer-logo">
            <span>${escHtml(clientName)}</span>
            <span>&nbsp;|&nbsp; ${escHtml(reportDate)}</span>
        </span>
    </div>`;
}

// ── Summary / title page ───────────────────────────────────────────────────────
function buildSummaryPage(rows, clientName, reportDate) {
    const totalPR = rows.reduce((s, d) => s + parseFloat(d.pr || 0), 0);
    const totalAD = rows.reduce((s, d) => s + parseFloat(d.ad || 0), 0);
    return `
    <div class="paper">
        <div class="report-title-block">
            <h1>Articles Report</h1>
            <div class="report-meta-row">
                <span><strong>Client:</strong> ${escHtml(clientName)}</span>
                <span><strong>Date:</strong> ${escHtml(reportDate)}</span>
                <span><strong>Items:</strong> ${rows.length}</span>
                <span><strong>Total PR Value:</strong> ${totalPR.toFixed(2)}</span>
                <span><strong>Total AD Value:</strong> ${totalAD.toFixed(2)}</span>
            </div>
        </div>
        ${footer(clientName, reportDate)}
        <div class="page-number"></div>
    </div>`;
}

// ── Single article page ───────────────────────────────────────────────────────
function buildArticlePage(d, idx, total, clientName, reportDate) {
    const rtl = isRtl(d.title || '');
    const titleCls = `media-card-title${rtl ? ' rtl-title' : ''}`;
    const titleHtml = d.articleurl
        ? `<a href="${escHtml(d.articleurl)}" target="_blank" rel="noopener">${escHtml(d.title||'Untitled')}</a>`
        : escHtml(d.title || 'Untitled');

    // decodeContent is global — defined in Index.cshtml @section Scripts
    const contentHtml = decodeContent(d.content || '');

    // screenshot is a single path string stored in data-screenshot
    const imgSrc = d.screenshot || '';

    const metaGrid = `
        <div class="media-meta-grid">
            ${mc('Date',              d.date)}
            ${mc('Category',          d.category)}
            ${mc('Media Tier',        d.mediatier)}

            ${mc('Website',           d.website)}
            ${mc('Sub Category',      d.subcategory)}
            ${mc('Media Type',        d.mediatype)}

            ${mc('Frequency',         d.frequency)}
            ${mc('AD Value',          d.ad)}
            ${mc('Language',          d.language)}

            ${mc('Impression',        d.impression)}
            ${mc('PR Value',          d.pr)}
            ${mc('Writer',            d.writer)}

            ${mc('Reach',             d.reach)}
            ${mc('Tone',              d.toning)}
            ${mc('Article Branding',  d.articlebrand)}

            ${mc('Headline Branding', d.headlinebrand)}
            ${mc('Picture in Article',d.picture)}
            ${mc('Generation',        d.generation)}

            ${mcLink('Article URL',   d.articleurl)}
        </div>`;

    const contentBlock = contentHtml ? `
        <div class="content-block">
            <div class="content-label">Content</div>
            <div class="content-body">${contentHtml}</div>
        </div>` : '';

    const inlineImage = imgSrc ? `
        <div class="inline-image">
            <img src="${escHtml(imgSrc)}" alt="${escHtml(d.title||'')}"
                 onerror="this.style.display='none'" />
        </div>` : '';

    return `
    <div class="paper">
        <div class="section">
            <div class="section-heading">
                <span>Online Articles</span>
                <span class="section-sub">${idx + 1} of ${total}</span>
            </div>
            <div class="media-card">
                <div class="${titleCls}">${titleHtml}</div>
                ${metaGrid}
                ${contentBlock}
                ${inlineImage}
            </div>
        </div>
        ${footer(clientName, reportDate)}
        <div class="page-number"></div>
    </div>`;
}

// ── viewAsHtml — called from Index toolbar ────────────────────────────────────
function viewAsHtml() {
    const rows = getSelectedRows();
    if (!rows.length) { alert('Please select at least one article.'); return; }

    const reportDate = new Date().toLocaleDateString('en-GB', { month:'long', year:'numeric' });
    const summaryPage   = buildSummaryPage(rows, clientName, reportDate);
    const articlePages  = rows.map((d, i) =>
        buildArticlePage(d, i, rows.length, clientName, reportDate)
    ).join('');

    const html = `<!DOCTYPE html>
<html lang="en" dir="ltr">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width,initial-scale=1" />
<title>${escHtml(clientName)} \u2014 Articles Report</title>
<style>${PREVIEW_CSS}</style>
</head>
<body>
  <div class="toolbar">
    <span class="toolbar-brand">EditorPR \u2014 Articles Report Preview</span>
    <div class="toolbar-actions">
      <button class="btn btn-print" onclick="window.print()">\uD83D\uDDA8 Print / Save PDF</button>
      <button class="btn" onclick="window.close()">\u2715 Close</button>
    </div>
  </div>
  <div class="page">
    ${summaryPage}
    ${articlePages}
  </div>
  <script>
    document.querySelectorAll('.paper .page-number').forEach(function(el, i) {
      el.textContent = 'Page ' + (i + 1);
    });
  <\/script>
</body>
</html>`;

    const win = window.open('', '_blank');
    if (!win) { alert('Please allow pop-ups to view the report.'); return; }
    win.document.open();
    win.document.write(html);
    win.document.close();
}
