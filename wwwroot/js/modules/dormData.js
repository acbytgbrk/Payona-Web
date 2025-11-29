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
    // Map gender values: Male/Erkek -> erkek, Female/Kadın -> kiz
    let genderKey = null;
    if (gender === 'Male' || gender === 'Erkek' || gender === 'male' || gender === 'erkek') {
        genderKey = 'erkek';
    } else if (gender === 'Female' || gender === 'Kadın' || gender === 'female' || gender === 'kiz') {
        genderKey = 'kiz';
    }
    
    if (!genderKey || !cityData[genderKey]) return [];
    
    return cityData[genderKey].sort();
}

