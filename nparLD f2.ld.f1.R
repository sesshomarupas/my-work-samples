#Installation der benötigten Packages
#install.packages("nparLD", dependencies = TRUE)
#install.packages("dplyr")
#install.packages("MASS")

# Pakete laden
library(MASS)
library(nparLD)
library(dplyr)

# Daten einlesen
data <- read.csv("C:/Users/spast/Desktop/nparLD f2.ld.f1/Data.csv", sep = ";")

# DataFrame mit wichtigen Variablen erstellen und Faktoren setzen
neuer_dataframe <- data.frame(
  Gruppe        = factor(data[["Gruppe.1...IG...2...KGinaktiv"]], levels = c(1, 2), labels = c("IG", "KG")),
  Geschlecht    = factor(data[["Geschlecht..m.1..w.2"]], levels = c(1, 2), labels = c("m", "w")),
  Altersgruppe  = factor(data[["Altersgruppe.1.60.64..2.65.69"]]),  # Optional, falls benötigt
  Messzeitpunkt = as.integer(data[["Messzeitpunkt"]]),
  Fallstab      = data[["Fallstab"]],
  Probanden     = factor(data[["Probanden_ID"]])
)

# Vorschau & Prüfung
print(neuer_dataframe)
# summary(neuer_dataframe)

#cat("Fehlende Werte in Fallstab:", sum(is.na(neuer_dataframe$Fallstab)), "\n")

# Nur vollständige Fälle mit genau 3 Messzeitpunkten behalten
vollstaendige_faelle <- neuer_dataframe %>%
  group_by(Probanden) %>%
  filter(n_distinct(Messzeitpunkt) == 3) %>%
  ungroup() %>%
  filter(!is.na(Fallstab)) %>%
  as.data.frame()

print(vollstaendige_faelle)
#write.csv(vollstaendige_faelle, "C:/Users/spast/Desktop/nparLD f2.ld.f1/vollstaendige_faelle.csv", row.names = FALSE)

# Argumente der Funktion prüfen
args(nparLD::f2.ld.f1)

# Nichtparametrische ANOVA mit Messwiederholung (2 Zwischenfaktoren, 1 Innersubjektfaktor)
result <- f2.ld.f1(
  y = vollstaendige_faelle$Fallstab,
  time = vollstaendige_faelle$Messzeitpunkt,
  group1 = vollstaendige_faelle$Gruppe,
  group2 = vollstaendige_faelle$Geschlecht,
  subject = vollstaendige_faelle$Probanden,
  description = TRUE,
  plot.RTE = TRUE
)

# Ergebnisse anzeigen
summary(result)
# p-Werte extrahieren
result$ANOVA.test
#result$ANOVA.test.mod.Box
result$Wald.test
result$RTE 

dataframe_results <- result$ANOVA.test
# Spalte 3 extrahieren, in numerisch umwandeln (falls sie als character vorliegt)
werte <- as.numeric(as.character(dataframe_results[, 3]))

# Auf 3 Nachkommastellen runden und als character (ohne scientific notation) formatieren
werte_formatiert <- format(round(werte, 3), scientific = FALSE)

# Ersetze Spalte 3 im Dataframe
dataframe_results[, 3] <- werte_formatiert

# Ergebnis anzeigen
print(dataframe_results)
---------------------------------------------------------------------------------------------

# Messzeitpunkte
library(dplyr)

paired_wilcox <- function(data, t1, t2) {
  values_t1 <- data %>% filter(Messzeitpunkt == t1) %>% arrange(Probanden) %>% pull(Fallstab)
  values_t2 <- data %>% filter(Messzeitpunkt == t2) %>% arrange(Probanden) %>% pull(Fallstab)
  test <- wilcox.test(values_t1, values_t2, paired = TRUE)
  return(test$p.value)
}

results_list <- list()

gruppen_geschlechter <- vollstaendige_faelle %>%
  distinct(Gruppe, Geschlecht)

for(i in seq_len(nrow(gruppen_geschlechter))) {
  grp <- gruppen_geschlechter$Gruppe[i]
  geschl <- gruppen_geschlechter$Geschlecht[i]
  
  data_sub <- vollstaendige_faelle %>%
    filter(Gruppe == grp, Geschlecht == geschl)
  
  if(length(unique(data_sub$Probanden)) > 1) {
    p_vals <- c(
      paired_wilcox(data_sub, 1, 2),
      paired_wilcox(data_sub, 1, 3),
      paired_wilcox(data_sub, 2, 3)
    )
    
    names(p_vals) <- c("1 vs 2", "1 vs 3", "2 vs 3")
    p_adj <- p.adjust(p_vals, method = "holm")
    
    # Auf 3 Nachkommastellen runden und keine wissenschaftliche Notation
    format_p <- function(x) {
      format(round(x, 3), nsmall = 3, scientific = FALSE)
    }
    
    results_list[[paste(grp, geschl, sep = "_")]] <- data.frame(
      Gruppe = grp,
      Geschlecht = geschl,
      Vergleich = names(p_vals),
      p_wert = format_p(p_vals),
      p_wert_adjustiert = format_p(p_adj),
      stringsAsFactors = FALSE
    )
  }
}

ergebnisse <- do.call(rbind, results_list)
print(ergebnisse)
