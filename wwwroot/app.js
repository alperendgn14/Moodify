// dil ve çeviri
const dict = {
    tr: {
        title: "🎵 Moodify <span class='highlight'>AI</span>",
        desc: "Nasıl hissediyorsun? Anlat, sana tam o ruh haline uygun şarkılar bulayım.",
        placeholder: "Örn: Çok enerjik hissediyorum, spora gideceğim...",
        btnSearch: "Şarkı Bul",
        btnLoading: "Yapay Zeka Düşünüyor...",
        resultsTitle: "Senin İçin Seçilenler",
        historyTitle: "Geçmiş Aramalar",
        themeDark: "Karanlık",
        themeLight: "Aydınlık",
        openSpotify: "Spotify'da Aç",
        album: "Albüm",
        releaseDate: "Çıkış",
        popularity: "Popülerlik Puanı"
    },
    en: {
        title: "🎵 Moodify <span class='highlight'>AI</span>",
        desc: "How are you feeling? Tell me, and I'll find the perfect songs for your mood.",
        placeholder: "E.g: I feel super energetic, about to hit the gym!",
        btnSearch: "Find Songs",
        btnLoading: "AI is thinking...",
        resultsTitle: "Picked for You",
        historyTitle: "Recent Searches",
        themeDark: "Dark",
        themeLight: "Light",
        openSpotify: "Open in Spotify",
        album: "Album",
        releaseDate: "Released",
        popularity: "Popularity Score"
    }
};

let currentLang = localStorage.getItem('lang') || 'tr';

function applyLanguage() {
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.innerHTML = dict[currentLang][key];
    });
    document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
        const key = el.getAttribute('data-i18n-placeholder');
        el.placeholder = dict[currentLang][key];
    });
    document.getElementById('langToggle').innerText = currentLang === 'tr' ? 'EN / TR' : 'TR / EN';
    updateThemeText();
}

document.getElementById('langToggle').addEventListener('click', () => {
    currentLang = currentLang === 'tr' ? 'en' : 'tr';
    localStorage.setItem('lang', currentLang);
    applyLanguage();
});

// tema
const toggleSwitch = document.querySelector('.theme-switch input[type="checkbox"]');
const currentTheme = localStorage.getItem('theme');
const themeText = document.getElementById('theme-text');

if (currentTheme) {
    document.body.classList.add(currentTheme);
    if (currentTheme === 'light-theme') toggleSwitch.checked = true;
}

function updateThemeText() {
    themeText.innerText = document.body.classList.contains('light-theme') ? dict[currentLang].themeLight : dict[currentLang].themeDark;
}

toggleSwitch.addEventListener('change', function (e) {
    if (e.target.checked) {
        document.body.classList.add('light-theme');
        localStorage.setItem('theme', 'light-theme');
    } else {
        document.body.classList.remove('light-theme');
        localStorage.setItem('theme', 'dark-theme');
    }
    updateThemeText();
});


// geçmiş aramalar
let searchHistory = JSON.parse(localStorage.getItem('searchHistory')) || [];

function renderHistory() {
    const historySection = document.getElementById('historySection');
    const historyTags = document.getElementById('historyTags');
    historyTags.innerHTML = '';

    if (searchHistory.length === 0) {
        historySection.style.display = 'none';
        return;
    }

    historySection.style.display = 'block';
    searchHistory.forEach(term => {
        const tag = document.createElement('div');
        tag.className = 'history-tag';
        tag.innerText = term;
        tag.onclick = () => {
            document.getElementById('moodInput').value = term;
            document.getElementById('searchBtn').click(); // otomatik arat
        };
        historyTags.appendChild(tag);
    });
}

function addToHistory(term) {
    if (!term || searchHistory.includes(term)) return;
    searchHistory.unshift(term);
    if (searchHistory.length > 5) searchHistory.pop(); // son 5 aramayı tut
    localStorage.setItem('searchHistory', JSON.stringify(searchHistory));
    renderHistory();
}


// şarkı detayları
const modal = document.getElementById('songModal');
const closeBtn = document.querySelector('.close-btn');

closeBtn.onclick = () => modal.style.display = "none";
window.onclick = (e) => { if (e.target === modal) modal.style.display = "none"; }

function openModal(song) {
    const modalBody = document.getElementById('modalBody');
    modalBody.innerHTML = `
        <img src="${song.kapakFotografi}" class="modal-album-art">
        <h2>${song.sarkiAdi}</h2>
        <p style="color: var(--text-sub);">${song.sanatci}</p>
        
        <div class="modal-stats">
            <div class="stat-box">
                <p>${dict[currentLang].album}</p>
                <h4>${song.albumAdi || '-'}</h4>
            </div>
            <div class="stat-box">
                <p>${dict[currentLang].releaseDate}</p>
                <h4>${song.yayinTarihi ? song.yayinTarihi.substring(0, 4) : '-'}</h4>
            </div>
            <div class="stat-box">
                <p>${dict[currentLang].popularity}</p>
                <h4>%${song.populerlik || '0'}</h4>
            </div>
        </div>
        
        <a href="${song.spotifyLinki}" target="_blank" class="spotify-btn" style="display:inline-block; margin-top:10px;">
            ${dict[currentLang].openSpotify}
        </a>
    `;
    modal.style.display = "flex";
}


// arama yapma
document.getElementById('searchBtn').addEventListener('click', async () => {
    const moodInput = document.getElementById('moodInput').value;
    const searchBtn = document.getElementById('searchBtn');
    const errorMessage = document.getElementById('errorMessage');
    const resultsSection = document.getElementById('resultsSection');
    const songsGrid = document.getElementById('songsGrid');

    if (!moodInput.trim()) return;

    addToHistory(moodInput.trim());

    searchBtn.disabled = true;
    searchBtn.querySelector('.btn-text').innerText = dict[currentLang].btnLoading;
    errorMessage.style.display = 'none';
    resultsSection.style.display = 'none';
    songsGrid.innerHTML = '';

    try {
        const response = await fetch('/api/MusicAgent/recommend', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userMood: moodInput, language: currentLang.toUpperCase() })
        });

        if (!response.ok) throw new Error('Sunucu hatası.');
        const data = await response.json();

        data.sarkilar.forEach((song, index) => {
            const card = document.createElement('div');
            card.className = 'song-card';
            card.style.animationDelay = `${index * 0.1}s`;
            card.style.cursor = "pointer"; // tıklanabilir hissi

            // karta tıklanınca modalı aç
            card.onclick = (e) => {
                // butona veya sese tıklandıysa modalı aç
                if (e.target.tagName !== 'A' && e.target.tagName !== 'AUDIO') {
                    openModal(song);
                }
            };

            let audioHtml = song.onizlemeSesi ? `<audio controls src="${song.onizlemeSesi}" class="audio-player"></audio>` : '';

            card.innerHTML = `
                <img src="${song.kapakFotografi}" alt="${song.sarkiAdi}" class="album-art" />
                <div class="song-info">
                    <h3>${song.sarkiAdi}</h3>
                    <p>${song.sanatci}</p>
                    <div class="actions">
                        <a href="${song.spotifyLinki}" target="_blank" class="spotify-btn">${dict[currentLang].openSpotify}</a>
                        ${audioHtml}
                    </div>
                </div>
            `;
            songsGrid.appendChild(card);
        });

        resultsSection.style.display = 'block';
    } catch (err) {
        errorMessage.innerText = err.message;
        errorMessage.style.display = 'block';
    } finally {
        searchBtn.disabled = false;
        searchBtn.querySelector('.btn-text').innerText = dict[currentLang].btnSearch;
    }
});

// sayfa geldiğinde ayarlar uygulansın.
applyLanguage();
renderHistory();