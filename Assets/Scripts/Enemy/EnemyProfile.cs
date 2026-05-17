/// <summary>
/// Perfil genético dominante de un enemigo.
/// Se determina en tiempo de ejecución comparando los 5 genes del cromosoma.
/// Controla el modelo visual, escala y comportamiento del enemigo.
/// </summary>
public enum EnemyProfile
{
    Balanced,    // Ningún gen destaca claramente — modelo: Drone VoodooPlay
    Speed,       // Gen speed dominante — modelo: AlienFighter (mediano, ágil)
    Aggression,  // Gen aggression dominante — modelo: AlienDestroyer (grande, amenazante)
    Vitality     // Gen vitality dominante — modelo: AlienDestroyer (igual, pero más grande)
}