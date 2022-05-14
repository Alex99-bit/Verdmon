using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterMecha : MonoBehaviour
{
    CharacterStats monstStats;
    Transform posPlayer;
    Vector3 dPlayer;
    Animator animator;
    Rigidbody2D rigidEnemy;
    public GameObject bullet;
    public GameObject canon, pies;
    public LayerMask enemyMask, ground;
    bool cercaPlayer, disparar, perseguir;
    public ParticleSystem sangre;
    public Slider barVida;

    const string IS_ALIVE = "isAlive", IS_RUNNING = "isRunning";

    // Start is called before the first frame update
    void Start()
    {
        monstStats = new CharacterStats("Monster","Mutante");
        rigidEnemy = GetComponent<Rigidbody2D>();
        posPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        animator = GetComponent<Animator>();
        barVida = GameObject.Find("VidaBoss").GetComponent<Slider>();
        StartCoroutine(Shooting());
        perseguir = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.currentGameState == GameState.inGame)
        {
            // Sigue la posision del player

            Debug.DrawRay(this.transform.position, Vector2.down * 2f, Color.red);
            Debug.DrawRay(pies.transform.position, Vector2.left * 1f, Color.red);
            dPlayer = posPlayer.transform.position - this.transform.position;

            Debug.DrawRay(transform.position, dPlayer, Color.red);
            PlayerCerca();

            if (perseguir)
            {
                Detener();
            }

            if (monstStats.getVida() <= 0)
            {
                // Muere el enemigos
                animator.SetBool(IS_ALIVE, false);
                rigidEnemy.gravityScale = 0.5f;
                Destroy(this.gameObject, 0.5f);

                GameManager.instance.SetGameState(GameState.gameOver);
            }
        }
        else if (GameManager.instance.currentGameState == GameState.inicio)
        {
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.currentGameState == GameState.inGame)
        {
            if (perseguir)
            {
                SeguirPlayer();
                Jump();
            }
        }
    }

    void SeguirPlayer()
    {
        if (!cercaPlayer)
        {
            // Si no esta demaciado cerca, el enemigo persigue al player
            transform.Translate(dPlayer * Time.deltaTime * monstStats.getSpeed());
            animator.SetBool(IS_RUNNING, true);
            disparar = false;
        }
        else if(cercaPlayer)
        {
            // Si esta lo suficioentemente serca, se detiene y dispara
            transform.Translate(dPlayer * Time.deltaTime * (monstStats.getSpeed() - 0.3f));
            animator.SetBool(IS_RUNNING, false);
            disparar = true;
            // Sonido de disparo

        }

        // Cambia el sprite del enemigo, dependiendo la posicion del player
        if (posPlayer.position.x > transform.position.x)
        {
            // El player esta a la derecha
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            // El player esta a la izquierda
            GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    void Detener()
    {
        if (Physics2D.Raycast(this.transform.position, dPlayer, 22f, enemyMask))
        {
            // Si el enemigo esta a ciertos metros del jugador, se detiene y dispara
            cercaPlayer = true;
        }
        else
        {
            cercaPlayer = false;
        }
    }

    IEnumerator Shooting()
    {
        while (GameManager.instance.currentGameState == GameState.inGame)
        {
            // Rutina para que el enemigo dispare
            yield return new WaitForSeconds(3f);
            if (disparar)
            {
                Instantiate(bullet, canon.transform);
            }
        }
    }

    void PlayerCerca()
    {
        // Retorna un true o false si esta cerca del jugador o no
        //Debug.DrawRay(transform.position, dPlayer, Color.red);
        if (Physics2D.Raycast(this.transform.position, dPlayer, 25f, enemyMask))
        {
            perseguir = true; // Vio al jugador
            //expandirCamara = true;
        }
        else
        {
            //expandirCamara = false;
            perseguir = false;
            animator.SetBool(IS_RUNNING, false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("BulletPlayer"))
        {
            // Se le resta vida en caso de que reciba un disparo
            monstStats.setVida(monstStats.getVida() - 15f);
            sangre.Play();
            barVida.value = monstStats.getVida();
        }
    }

    void Jump()
    {
        if (IsTochingTheGround())
        {
            // Si hay algo delante, salta
            if (!GetComponent<SpriteRenderer>().flipX && (Physics2D.Raycast(pies.transform.position, Vector2.left, 1f, ground)))
            {
                rigidEnemy.AddForce(Vector2.up * monstStats.getJumpForce(), ForceMode2D.Impulse);
            }
            else if (GetComponent<SpriteRenderer>().flipX && (Physics2D.Raycast(pies.transform.position, Vector2.right, 1f, ground)))
            {
                rigidEnemy.AddForce(Vector2.up * monstStats.getJumpForce(), ForceMode2D.Impulse);
            }
        }
    }

    bool IsTochingTheGround()
    {
        if (Physics2D.Raycast(pies.transform.position, Vector2.down, 0.5f, ground))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
