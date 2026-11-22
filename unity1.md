# Unity 6. Практическое занятие: предметы, враги и здоровье персонажа

**Формат:** последовательный практикум для студентов.  
**Итог:** игрок собирает предметы, враг преследует его и наносит урон при контакте, сердечко восстанавливает здоровье, показатель HP отображается на экране.

> **Правило работы:** выполняйте этапы по очереди. Код на следующем этапе иногда **заменяет** предыдущую версию того же файла — это нормальная часть занятия. Не добавляйте второй компонент с тем же назначением. Перед каждым этапом сохраняйте сцену.

## Что уже должно быть готово

- Сцена с землёй и платформами, у которых есть коллайдеры и выключен `Is Trigger`.
- Игрок с тегом `Player`, компонентом `CharacterController` и рабочим `PlayerController.cs`.
- `Main Camera` с рабочим `CameraFollow.cs`, который следит за игроком.
- В проекте используется Unity 6. Для движения врага ниже применяется `Rigidbody.linearVelocity`.
- Скрипты движения игрока и камеры **не изменяем**.

---

# Этап 1. Создаём собираемый объект

**Цель:** при соприкосновении с предметом он исчезает, а в Console появляется сообщение.

### Настройка Unity

1. Поместите на сцену модель предмета (например, сердечко). Назовите `heart`.
2. Добавьте `Box Collider`, включите `Is Trigger`.
3. Создайте скрипт `Collectible.cs` и прикрепите к `heart`.
4. Убедитесь, что на корневом объекте игрока установлен тег `Player`.

### Collectible.cs — версия 1

```csharp
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Предмет собран!");
            Destroy(gameObject);
        }
    }
}
```

**Обсудить:** `OnTriggerEnter` вызывается при входе в триггер; `CompareTag` определяет, кто вошёл; `Destroy` удаляет объект.

**Проверка:** запустите сцену и коснитесь предмета. Он должен исчезнуть. Если нет — проверьте тег `Player`, `Is Trigger`, расположение коллайдера и наличие `CharacterController` у игрока.

---

# Этап 2. Создаём врага

**Цель:** разместить физический объект врага, который будет сталкиваться с землёй и стенами.

### Настройка Unity

1. Добавьте модель врага, назовите корневой объект `Enemy`.
2. На корневой объект добавьте `Rigidbody` и `Box Collider` (или подходящий `Capsule Collider`). Подгоните размеры коллайдера под модель.
3. У `Rigidbody` включите `Use Gravity`, **выключите** `Is Kinematic`.
4. В `Constraints` включите `Freeze Rotation X` и `Freeze Rotation Z`, чтобы враг не заваливался набок.
5. В `Box Collider` выключите `Is Trigger`.
6. Разместите врага **над землёй**, чтобы он опустился на поверхность при запуске.

**Проверка:** нажмите Play. Враг падает на землю, но не проходит сквозь платформы. Если проходит — проверьте коллайдеры земли и врага, слои в `Project Settings → Physics`, а также `Is Kinematic`.

> Важно: динамический `Rigidbody` нужен для физического столкновения с препятствиями. Не передвигайте врага каждый кадр через `transform.position`, игнорируя физику.

---

# Этап 3. Учим врага преследовать игрока

**Цель:** враг начинает двигаться, когда игрок подходит на определённое расстояние.

Создайте `EnemyAI.cs` и прикрепите его к объекту `Enemy`.

### EnemyAI.cs

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float chaseRange = 10f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        Vector3 direction = player.position - rb.position;
        direction.y = 0f;

        Vector3 velocity = rb.linearVelocity;

        if (direction.magnitude > chaseRange || direction.sqrMagnitude < 0.01f)
        {
            velocity.x = 0f;
            velocity.z = 0f;
            rb.linearVelocity = velocity;
            return;
        }

        direction.Normalize();

        velocity.x = direction.x * speed;
        velocity.z = direction.z * speed;
        rb.linearVelocity = velocity;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            10f * Time.fixedDeltaTime
        ));
    }
}
```

### Настройка Inspector

- `Player`: перетащите корневой объект игрока из Hierarchy.
- `Speed`: `2`.
- `Chase Range`: `10`.

**Обсудить:** `FixedUpdate` работает с физикой; `direction` задаёт направление; `direction.y = 0` не даёт врагу лететь вверх за игроком; `linearVelocity` меняет скорость движения, сохраняя вертикальную скорость от гравитации.

**Проверка:** вдали враг стоит; на расстоянии менее 10 единиц он бежит к игроку; у стены упирается в неё, а не проходит сквозь неё. Обхода стен пока нет: для этого позднее потребуется поиск пути.

---

# Этап 4. Реагируем на контакт врага и игрока

**Цель:** при реальном столкновении выводить сообщение об атаке. Пока здоровье не уменьшаем — сначала проверим контакт.

У игрока используется `CharacterController`, поэтому удобно обрабатывать столкновение в `OnControllerColliderHit`.

### EnemyAttack.cs — версия 1

Создайте скрипт и прикрепите его к корневому объекту `Enemy`.

```csharp
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackRate = 1f;
    private float nextAttackTime;

    public void TryAttack()
    {
        if (attackRate <= 0f || Time.time < nextAttackTime)
            return;

        Debug.Log("Враг атакует игрока!");
        nextAttackTime = Time.time + 1f / attackRate;
    }
}
```

### PlayerEnemyCollision.cs — версия 1

Создайте скрипт и прикрепите к **корневому** объекту игрока, где установлен `CharacterController`.

```csharp
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerEnemyCollision : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        EnemyAttack enemy =
            hit.collider.GetComponentInParent<EnemyAttack>();

        if (enemy != null)
            enemy.TryAttack();
    }
}
```

**Обсудить:** физическое касание отличается от проверки расстояния через `OverlapSphere`; `attackRate = 1` ограничивает атаку примерно одним срабатыванием в секунду, пока контакт продолжается.

**Проверка:** подойдите к врагу: в Console появляются сообщения. Отойдите — сообщения прекращаются. Враг пока **не уменьшает HP**, это следующий этап.

> Срабатывание `OnControllerColliderHit` происходит при движении `CharacterController.Move()`. Поэтому проверяйте контакт в реальном Play Mode, когда враг прижимается к игроку и игрок продолжает двигаться. Если нужен гарантированный урон от неподвижного касания, позже можно перейти на отдельный триггер атаки.

---

# Этап 5. Добавляем здоровье и настоящий урон

**Цель:** игрок начинает со 100 HP и теряет 10 HP при атаке. На нуле здоровье не становится отрицательным.

### PlayerHealth.cs — версия 1

Создайте скрипт и прикрепите его к **корневому** объекту игрока.

```csharp
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Здоровье: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log("Здоровье: " + currentHealth);

        if (currentHealth == 0)
        {
            Debug.Log("Игра окончена!");

            PlayerController movement = GetComponent<PlayerController>();
            if (movement != null)
                movement.enabled = false;
        }
    }
}
```

> **Почему не `Destroy(gameObject)`?** Если удалить игрока, камера потеряет объект, за которым следит. Пока на нуле HP просто отключаем управление. Позже можно сделать экран Game Over и перезапуск уровня.

### EnemyAttack.cs — версия 2 (заменить версию 1)

```csharp
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 10;
    public float attackRate = 1f;
    private float nextAttackTime;

    public void TryAttack(PlayerHealth health)
    {
        if (health == null || attackRate <= 0f || Time.time < nextAttackTime)
            return;

        health.TakeDamage(damage);
        nextAttackTime = Time.time + 1f / attackRate;
    }
}
```

### PlayerEnemyCollision.cs — версия 2 (заменить версию 1)

```csharp
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerEnemyCollision : MonoBehaviour
{
    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        EnemyAttack enemy =
            hit.collider.GetComponentInParent<EnemyAttack>();

        if (enemy != null)
            enemy.TryAttack(health);
    }
}
```

### Настройка Inspector

- Игрок → `PlayerHealth`: `Max Health = 100`.
- Враг → `EnemyAttack`: `Damage = 10`, `Attack Rate = 1`.

**Проверка:** приближайтесь к врагу; следите в Console за значениями 90, 80, 70… При 0 управление отключается, но персонаж остаётся в сцене, и камера не теряет цель.

---

# Этап 6. Сердечко восстанавливает здоровье

**Цель:** уже знакомый собираемый объект теперь лечит игрока на 25 HP. Если здоровье полное, сердечко остаётся.

### PlayerHealth.cs — версия 2 (добавить метод в существующий класс)

Добавьте внутрь класса `PlayerHealth`, **после** `TakeDamage`:

```csharp
public bool Heal(int amount)
{
    if (currentHealth <= 0 || currentHealth >= maxHealth || amount <= 0)
        return false;

    currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    Debug.Log("Здоровье восстановлено: " + currentHealth);
    return true;
}
```

### Collectible.cs — версия 2 (заменить версию 1)

```csharp
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int healAmount = 25;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null && health.Heal(healAmount))
        {
            Debug.Log("Сердечко собрано!");
            Destroy(gameObject);
        }
    }
}
```

В Inspector у сердечка установите `Heal Amount = 25`. Его `Box Collider` должен оставаться с включённым `Is Trigger`.

**Обсудить:** `bool` возвращает `true`, только когда удалось восстановить HP; `Mathf.Min` ограничивает здоровье максимумом.

**Проверка:** при 100 HP сердечко не исчезает; после 3 атак (70 HP) сердечко восстанавливает 25 HP (до 95 HP) и исчезает.

---

# Этап 7. Отображаем показатель здоровья на экране

**Цель:** отображать простой текст `Здоровье: 100 / 100`, который изменяется при атаке и подборе сердечка. **Без Slider и полоски здоровья.**

### Создание UI

1. В Hierarchy: `Create → UI → Text - TextMeshPro`. Если Canvas отсутствует, Unity создаст его.
2. Переименуйте текст в `HealthText`.
3. У `Canvas` установите `Render Mode = Screen Space - Overlay`.
4. У `HealthText` в `Rect Transform` выберите **Top Left** anchor, задайте размер около `350 × 60` и расположите текст в верхнем левом углу экрана.
5. В `TextMeshPro - Text (UI)` найдите поле `Text Input`, введите `Здоровье: 100 / 100`, задайте подходящий размер шрифта (например, 30–36).

### PlayerHealth.cs — версия 3 (добавить отображение)

Добавьте в начало файла после `using UnityEngine;`:

```csharp
using TMPro;
```

Внутри класса добавьте поля:

```csharp
public TMP_Text healthText;
```

В `Start()` после `currentHealth = maxHealth;` добавьте:

```csharp
UpdateHealthText();
```

В `TakeDamage()` сразу после изменения `currentHealth` добавьте:

```csharp
UpdateHealthText();
```

В `Heal()` сразу после изменения `currentHealth` добавьте:

```csharp
UpdateHealthText();
```

В конец класса добавьте метод:

```csharp
private void UpdateHealthText()
{
    if (healthText != null)
        healthText.text = "Здоровье: " + currentHealth + " / " + maxHealth;
}
```

### Inspector

Выберите игрока → компонент `PlayerHealth` → поле `Health Text` → перетащите `HealthText` из Hierarchy. **Не забудьте подключить ссылку:** без неё код не покажет изменения текста.

**Проверка:** при старте видно `Здоровье: 100 / 100`; атака даёт `90 / 100`; сердечко прибавляет HP; при максимальном здоровье значение не превышает 100.

---

# Итоговая проверка проекта

- Предмет собирается при входе в триггер, если это игрок.
- Враг стоит на земле и не проходит сквозь обычные платформы.
- Враг начинает преследование в пределах `Chase Range`.
- Урон наносится при контакте, а не издалека.
- Здоровье игрока уменьшается и не становится отрицательным.
- Сердечко восстанавливает HP, но не выше максимума.
- На экране виден текст текущего и максимального здоровья.
- `PlayerController.cs` и `CameraFollow.cs` остаются прежними.

## Типичные ошибки

| Симптом | Что проверить |
|---|---|
| Предмет не собирается | `Is Trigger`, тег `Player`, пересечение коллайдеров |
| Враг проваливается или проходит через стену | `Rigidbody.Is Kinematic = false`, коллайдеры, Collision Matrix |
| Враг не преследует | Назначено ли поле `EnemyAI.Player`, хватает ли `Chase Range` |
| Враг наносит урон издалека | Удалён ли старый вариант `EnemyAttack` с `Physics.OverlapSphere()` |
| HP не меняется | На игроке есть `PlayerHealth` и `PlayerEnemyCollision`; на враге `EnemyAttack` |
| Сердечко не исчезает | Возможно, здоровье уже полное — это ожидаемое поведение |
| Текст остаётся «New Text» | Поле `PlayerHealth.Health Text` не назначено или метод обновления не вызывается |
| Камера перестала следить | Не удаляйте игрока на 0 HP; проверьте `CameraFollow.Target` |

## Мини-задания для самостоятельной работы

1. Измените урон врага с 10 до 5 и убедитесь, что HP убывают медленнее.
2. Добавьте два сердечка: одно восстанавливает 10 HP, другое — 40 HP.
3. Настройте радиус преследования у двух врагов так, чтобы они замечали игрока на разном расстоянии.
4. Измените цвет и расположение `HealthText`, не меняя скрипт.

**Примечание преподавателю.** `OnControllerColliderHit` подходит для данного проекта, где игрок постоянно перемещается через `CharacterController.Move`, однако для более сложной боевой системы и гарантированного урона от неподвижного контакта предпочтительнее отдельный `Trigger` атаки или контролируемая проверка касания.
