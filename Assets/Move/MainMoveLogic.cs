using UnityEngine;
using UnityEngine.Events;

public class MainMoveLogic : MonoBehaviour
{
	void OnDrawGizmosSelected()
	{
		if (checkGround == null) return;
		Gizmos.DrawWireSphere(checkGround.position, checkRadius);
		if (checkGround == null) return;
		Gizmos.DrawWireSphere(checkCeiling.position, checkRadiusTwo);
	}
	[Header("State")]
	public float jumpForce = 400f;       // Количество силы, добавленной при прыжке игрока.
	public float crouchSpeed = 0.36f;    // Количество maxSpeed, примененное к крадущемуся движению в присяде. 1 = 100%
	public float Smoothing = 0.1f;       // Задержка между движениями
	public bool m_AirControl = false;    // Может ли игрок управлять во время прыжка

	[Header("Other")]
	public LayerMask whatIsGround;       // Слой, определяющая, что персонаж на земле
	public Transform checkGround;
	public Transform checkCeiling;
	public Collider2D crouchOffCollider; // Коллайдер, который будет отключен при приседании

	public float checkRadius = .2f;      // Радиус круга , чтобы определить, на земле ли
	public bool isGroud;                 // На земле ли игрок
	public float checkRadiusTwo = .2f;   //  Радиус круга перекрытия, чтобы определить, может ли игрок встать
	private Rigidbody2D m_Rigidbody2D;
	private bool facingRight = true;   // Для определения направления движения игрока
	private Vector3 m_Velocity = Vector3.zero;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool onCrouching = false;



	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void Update()
	{
		bool wasGrounded = isGroud;
		isGroud = false;

		// Игрок приземляется, если круговой радиус в позиции проверки земли задевает что-либо, обозначенное как земля
		// Это можно сделать, используя слои.    
		Collider2D[] colliders = Physics2D.OverlapCircleAll(checkGround.position, checkRadius, whatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				isGroud = true;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}
	}


	public void Move(float move, bool crouch, bool jump)
	{
		// Если присел, проверьте, может ли персонаж встать
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(checkCeiling.position, checkRadiusTwo, whatIsGround))
			{
				crouch = true;
			}
		}

		// управлять игроком только если он призимлился или airControl включен
		if (isGroud || m_AirControl)
		{

			// Если присел
			if (crouch)
			{
				if (!onCrouching)
				{
					onCrouching = true;
					OnCrouchEvent.Invoke(true);
				}
				// Уменьшается скорость с помощью множителя crouchSpeed
				move *= crouchSpeed;
				// Отключаем один из коллайдеров при присидании
				if (crouchOffCollider != null)
					crouchOffCollider.enabled = false;
			}
			else
			{
				// Включаем колайдер,когда не присел
				if (crouchOffCollider != null)
					crouchOffCollider.enabled = true;

				if (onCrouching)
				{
					onCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}
			// Переместить персонажа, найдя целевую скорость
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// А затем сгладить его и применить к персонажу
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity,
				targetVelocity, ref m_Velocity, Smoothing);

			// Если input перемещает игрока вправо и игрок направлен влево... 
			if (move > 0 && !facingRight)
			{
				//  Меняем направление.
				Swap();
			}
			// В противном случае, если input перемещает игрока влево, 
			//  а игрок направлен вправо ...
			else if (move < 0 && facingRight)
			{
				//  Меняем направление.
				Swap();
			}

		}
		// Если игрок должен прыгнуть
		if (isGroud && jump)
		{
			// Добавляем вертикальную силу к игроку

			isGroud = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, jumpForce));
			GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

		}
	}

	//Меняем направление
	private void Swap()
	{
		// Переключить переменную направления лица игрока
		facingRight = !facingRight;

		// Разворачиваем его
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}
}
