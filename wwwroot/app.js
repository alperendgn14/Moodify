// --- TEMA YÖNETİMİ ---
const toggleSwitch = document.querySelector('.theme-switch input[type="checkbox"]');
const currentTheme = localStorage.getItem('theme');
const themeText = document.getElementById('theme-text');

// Sayfa yüklendiğinde eski temayı hatırla
if (currentTheme) {
    document.body.classList.add(currentTheme);
    if (currentTheme === 'light-theme') {
        toggleSwitch.checked = true;
        themeText.innerText = "Aydınlık";
    }
}

// Anahtara tıklandığında temayı değiştir
toggleSwitch.addEventListener('change', function (e) {
    if (e.target.checked) {
        document.body.classList.add('light-theme');
        localStorage.setItem('theme', 'light-theme');
        themeText.innerText = "Aydınlık";
    } else {
        document.body.classList.remove('light-theme');
        localStorage.setItem('theme', 'dark-theme');
        themeText.innerText = "Karanlık";
    }
});


// --- ŞARKI ARAMA MANTIĞI ---
document.getElementById('searchBtn').addEventListener('click', async () => {
    const moodInput = document.getElementById('moodInput').value;
    const searchBtn = document.getElementById('searchBtn');
    const errorMessage = document.getElementById('errorMessage');
    const resultsSection = document.getElementById('resultsSection');
    const songsGrid = document.getElementById('songsGrid');

    if (!moodInput.trim()) return;

    searchBtn.disabled = true;
    searchBtn.querySelector('.btn-text').innerText = 'Yapay Zeka Düşünüyor...';
    errorMessage.style.display = 'none';
    resultsSection.style.display = 'none';
    songsGrid.innerHTML = '';

    try {
        const response = await fetch('https://localhost:7133/api/MusicAgent/recommend', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userMood: moodInput, language: 'TR' })
        });

        if (!response.ok) throw new Error('Sunucuya bağlanırken bir hata oluştu.');
        const data = await response.json();

        // Şarkıları ekrana basarken "Gecikmeli Animasyon" ekliyoruz
        data.sarkilar.forEach((song, index) => {
            const card = document.createElement('div');
            card.className = 'song-card';

            // Her kart bir öncekinden 0.1 saniye daha geç ekrana kayarak girecek
            card.style.animationDelay = `${index * 0.1}s`;

            let audioPlayerHtml = '';
            if (song.onizlemeSesi) {
                audioPlayerHtml = `<audio controls src="${song.onizlemeSesi}" class="audio-player"></audio>`;
            }

            card.innerHTML = `
                <img src="${song.kapakFotografi}" alt="${song.sarkiAdi}" class="album-art" />
                <div class="song-info">
                    <h3>${song.sarkiAdi}</h3>
                    <p>${song.sanatci}</p>
                    <div class="actions">
                        <a href="${song.spotifyLinki}" target="_blank" class="spotify-btn">Spotify'da Aç</a>
                        ${audioPlayerHtml}
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
        searchBtn.querySelector('.btn-text').innerText = 'Şarkı Bul';
    }
});