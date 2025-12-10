/**
 * Dormitory Data Module - Loads and manages KYK dormitory data
 */

let dormData = null;

export async function loadDormData() {
    if (dormData) {
        return dormData;
    }
    
    try {
        const response = await fetch('/data/kyk_yurtlar_turkiye.json');
        dormData = await response.json();
        return dormData;
    } catch (error) {
        console.error('Error loading dorm data:', error);
        return {};
    }
}

export function getCities() {
    if (!dormData) return [];
    return Object.keys(dormData).sort();
}

export function getDormsForCity(city, gender) {
    if (!dormData || !dormData[city]) return [];
    
    const cityData = dormData[city];
    const dorms = new Set(); // Duplicate'leri önlemek için Set kullanıyoruz
    
    // Map gender values: Male/Erkek -> erkek, Female/Kadın -> kiz
    let genderKey = null;
    if (gender === 'Male' || gender === 'Erkek' || gender === 'male' || gender === 'erkek') {
        genderKey = 'erkek';
    } else if (gender === 'Female' || gender === 'Kadın' || gender === 'female' || gender === 'kiz') {
        genderKey = 'kiz';
    }
    
    // Cinsiyete özel yurtları ekle
    if (genderKey && cityData[genderKey]) {
        cityData[genderKey].forEach(dorm => dorms.add(dorm));
    }
    
    // Karışık yurtları ekle (hem erkek hem kız listesinde olan yurtlar)
    if (cityData.erkek && cityData.kiz) {
        const erkekSet = new Set(cityData.erkek);
        const kizSet = new Set(cityData.kiz);
        
        // Her iki listede de olan yurtlar karışık yurtlardır
        erkekSet.forEach(dorm => {
            if (kizSet.has(dorm)) {
                dorms.add(dorm);
            }
        });
    }
    
    // Other veya Other-Mixed durumunda tüm yurtları göster
    if (gender === 'Other' || gender === 'Other-Mixed' || !genderKey) {
        if (cityData.erkek) {
            cityData.erkek.forEach(dorm => dorms.add(dorm));
        }
        if (cityData.kiz) {
            cityData.kiz.forEach(dorm => dorms.add(dorm));
        }
    }
    
    return Array.from(dorms).sort();
}

