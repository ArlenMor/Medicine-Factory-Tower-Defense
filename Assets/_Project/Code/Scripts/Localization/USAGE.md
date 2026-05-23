# Localization System — Usage Guide

## 1. Создание LocalizationConfig

1. В папке `Assets/_Project/Content/Configs/` нажми ПКМ → **Create → Localization → LocalizationConfig**
2. Назови `LocalizationConfig`
3. В инспекторе заполни `Entries` — список всех строк

### Формат записи:
| Key | English | Russian | EnglishSprite | RussianSprite |
|-----|---------|---------|---------------|---------------|
| `level_label` | Level: {0}/{1} | Уровень: {0}/{1} | — | — |
| `tutorial_img_focus` | — | — | Focus_en | Focus_ru |

`{0}`, `{1}` — плейсхолдеры для подстановки значений (format args).

---

## 2. Подключение в GameConfig

1. Открой `GameConfig` ассет (лежит рядом с `LocalizationConfig`)
2. Перетащи `LocalizationConfig` в поле `LocalizationConfig`

---

## 3. Локализация TMP-текста (без кода)

Проще всего — добавить компонент `LocalizedTMPro` на `GameObject` с `TMP_Text`:

1. Выбери объект в сцене (например, `_titleText` в `GameOverPanel`)
2. Add Component → `Localized TMPro`
3. В поле `Key` введи ключ из `LocalizationConfig` (например, `gameover_victory`)

Готово. Текст подтянется автоматически при старте и при смене языка.

### Плейсхолдеры {0} в TMP
Для TMP с динамическими значениями используй программный подход (см. п.4).

---

## 4. Локализация в коде

```csharp
var loc = S.Get<LocalizationService>();

// Простой текст
_titleText.text = loc.GetString("gameover_victory");

// С плейсхолдерами
_levelText.text = loc.GetString("level_label", _currentLevel, totalLevels);

// Название медикамента
var name = loc.GetMedicationName(task.ResultType);

// Спрайт (для картинок с текстом)
_image.sprite = loc.GetSprite("tutorial_img_focus");
```

Подписка на смену языка (если текст обновляется вручную):
```csharp
private LocalizationService _loc;

private void Start()
{
    _loc = S.Get<LocalizationService>();
    _loc.OnLocaleChanged += RefreshText;
}

private void RefreshText()
{
    _myText.text = _loc.GetString("my_key");
}
```

---

## 5. Локализация туториала

1. Открой `TutorialStep` ассет (например, `Level1Step1.asset`)
2. Заполни поле `Localization Key` (например, `tutorial_l1_s1`)
3. `InstructionText` остаётся как fallback — если ключ не задан, используется он
4. Добавь записи в `LocalizationConfig`:
- `tutorial_l1_s1` → `"Welcome! / Добро пожаловать!"`
- `tutorial_l1_s2` → `"... / ..."`

---

## 6. Кнопка смены языка

1. Создай `Button` в сцене
2. Add Component → `Language Switch Button`
3. В поле `Target Language` выбери `En` или `Ru`

После нажатия все `LocalizedTMPro`, `LocalizedImage` и подписчики `OnLocaleChanged` обновятся автоматически.

---

## 7. Картинки с текстом (LocalizedImage)

Для UI `Image`, содержащего текст на картинке:

1. Add Component → `Localized Image`
2. В поле `Key` введи ключ (например, `tutorial_img_focus`)
3. В `LocalizationConfig` для этого ключа заполни `EnglishSprite` и `RussianSprite`

При смене языка спрайт подменится автоматически.

---

## 8. Список всех ключей

Уже вшиты в код. Вот что нужно добавить в `LocalizationConfig`:

### Игровые строки
| Key | EN | RU |
|-----|----|----|
| `level_label` | Level: {0}/{1} | Уровень: {0}/{1} |
| `order_label` | Order: {0} | Заказ: {0} |
| `goal_label` | Goal: {0}/{1} | Цель: {0}/{1} |
| `task_complete` | --- | --- |
| `gameover_victory` | Victory! | Победа! |
| `gameover_defeat` | Defeat! | Поражение! |
| `gameover_next_level` | Next Level | Следующий уровень |
| `gameover_retry_level` | Retry Level {0} | Повторить уровень {0} |
| `stat_enemies_killed` | Enemies killed: {0} | Врагов убито: {0} |
| `stat_resources_collected` | Resources collected: {0} | Ресурсов собрано: {0} |
| `stat_plants_planted` | Plants planted: {0} | Растений посажено: {0} |
| `stat_credits_earned` | Credits earned: {0} | Кредитов заработано: {0} |
| `stat_time_played` | Time played: {0} | Времени прошло: {0} |
| `stat_upgrades_purchased` | Upgrades purchased: {0} | Улучшений куплено: {0} |
| `stat_turrets_built` | Turrets built: {0} | Турелей построено: {0} |
| `stat_barricades_built` | Barricades built: {0} | Баррикад построено: {0} |
| `plant_productivity` | Productivity\n{0:0.#} p/s | Производительность\n{0:0.#} в сек |
| `plant_grow_time` | Grow Time\n{0:0.#} sec | Время роста\n{0:0.#} сек |
| `upgrade_max` | MAX: {0} | МАКС: {0} |
| `upgrade_zero_percent` | 0% | 0% |
| `upgrade_percent` | {0:0.#}% | {0:0.#}% |
| `upgrade_multiplier` | x{0} | x{0} |

### Названия медикаментов
| Key | EN | RU |
|-----|----|----|
| `medication_GelPotion` | Gel Potion | Гелевое зелье |
| `medication_GelСapsules` | Gel Capsules | Гелевые капсулы |
| `medication_GelSyringe` | Gel Syringe | Гелевый шприц |
| `medication_CryustalGelSyringe` | Crystal Gel Syringe | Кристально-гелевый шприц |
| `medication_CrystalSyringe` | Crystal Syringe | Кристальный шприц |
| `medication_CrystalСapsulesBox` | Crystal Capsules Box | Коробка кристальных капсул |
| `medication_CrystalСapsulesRaw` | Crystal Capsules Raw | Сырые кристальные капсулы |
| `medication_CrystalPlastine` | Crystal Plastine | Кристальная пластина |
| `medication_PolymerСapsules` | Polymer Capsules | Полимерные капсулы |
| `medication_PolymerBandage` | Polymer Bandage | Полимерный бинт |
| `medication_PolymerСapsulesBox` | Polymer Capsules Box | Коробка полимерных капсул |
| `medication_FirstAidKit` | First Aid Kit | Аптечка |

---

## 9. Проверка при добавлении нового текста

1. Добавь ключ в `LocalizationConfig` (EN + RU)
2. Если текст в TMP — кинь `LocalizedTMPro` и укажи ключ
3. Если текст в коде — используй `S.Get<LocalizationService>().GetString("key", args)`
4. Если текст на картинке — кинь `LocalizedImage` и укажи ключ + спрайты
