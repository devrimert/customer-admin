/**
 * All German UI texts. Components read texts from here instead of writing them inline,
 * so a second language can be added later without touching the components.
 */
export const de = {
  app: {
    title: 'Kundenverwaltung',
  },

  nav: {
    customers: 'Kunden',
    newCustomer: 'Neuer Kunde',
  },

  common: {
    save: 'Speichern',
    cancel: 'Abbrechen',
    edit: 'Bearbeiten',
    delete: 'Löschen',
    back: 'Zurück',
    loading: 'Wird geladen …',
    saving: 'Wird gespeichert …',
    unsavedChanges: 'Es gibt ungespeicherte Änderungen. Wirklich schließen?',
    empty: '–',
  },

  notFound: {
    title: 'Seite nicht gefunden',
    backToList: 'Zur Kundenliste',
  },

  customers: {
    title: 'Kunden',
    searchPlaceholder: 'Nach Kunde oder Ansprechpartner suchen',
    showInactive: 'Inaktive anzeigen',
    empty: 'Keine Kunden gefunden.',
    active: 'Aktiv',
    inactive: 'Inaktiv',
    deactivate: 'Deaktivieren',
    activate: 'Aktivieren',
    editTitle: 'Kunde bearbeiten',
    newTitle: 'Neuer Kunde',
    confirmDelete: (name: string) =>
      `Kunde „${name}“ und alle Ansprechpartner endgültig löschen?`,
    fields: {
      name: 'Name',
      city: 'Ort',
      phone: 'Telefon',
      email: 'E-Mail',
      street: 'Straße',
      postalCode: 'PLZ',
      note: 'Notiz',
      contactCount: 'Ansprechpartner',
      status: 'Status',
      createdAt: 'Angelegt am',
      updatedAt: 'Zuletzt geändert',
    },
  },

  contacts: {
    title: 'Ansprechpartner',
    new: 'Neuer Ansprechpartner',
    editTitle: 'Ansprechpartner bearbeiten',
    empty: 'Noch keine Ansprechpartner.',
    primary: 'Hauptansprechpartner',
    makePrimary: 'Als Hauptansprechpartner festlegen',
    confirmDelete: (name: string) => `Ansprechpartner „${name}“ endgültig löschen?`,
    fields: {
      name: 'Name',
      firstName: 'Vorname',
      lastName: 'Nachname',
      position: 'Position',
      phone: 'Telefon',
      email: 'E-Mail',
      note: 'Notiz',
    },
  },

  validation: {
    required: 'Bitte ausfüllen.',
    email: 'Bitte eine gültige E-Mail-Adresse eingeben.',
    phone: 'Bitte nur Ziffern, Leerzeichen und + - / ( ) verwenden.',
    tooShort: (min: number) => `Mindestens ${min} Zeichen.`,
    tooLong: (max: number) => `Höchstens ${max} Zeichen.`,
  },

  errors: {
    network: 'Der Server ist nicht erreichbar. Bitte später erneut versuchen.',
    invalid: 'Bitte die Eingaben prüfen.',
    notFound: 'Der Eintrag wurde nicht gefunden. Vielleicht wurde er gelöscht.',
    conflict: 'Der Eintrag wurde inzwischen von jemand anderem geändert. Bitte die Seite neu laden.',
    unexpected: 'Ein unerwarteter Fehler ist aufgetreten.',
  },
} as const
