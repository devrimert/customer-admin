import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { errorMessage } from '../../api/errors'
import type { Contact } from '../../api/types'
import FormField from '../../components/FormField'
import Modal from '../../components/Modal'
import { useUnsavedChangesWarning } from '../../hooks/useUnsavedChangesWarning'
import { t } from '../../i18n'
import { contactSchema, toContactRequest, type ContactFormValues } from './contactSchema'
import { useCreateContact, useUpdateContact } from './queries'

type ContactFormDialogProps = {
  customerId: number
  // Without a contact, the form creates a new one
  contact?: Contact
  onClose: () => void
}

const FORM_ID = 'contact-form'

export default function ContactFormDialog({ customerId, contact, onClose }: ContactFormDialogProps) {
  const create = useCreateContact(customerId)
  const update = useUpdateContact(customerId)
  const save = contact ? update : create

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
  } = useForm<ContactFormValues>({
    resolver: zodResolver(contactSchema),
    defaultValues: {
      firstName: contact?.firstName ?? '',
      lastName: contact?.lastName ?? '',
      position: contact?.position ?? '',
      email: contact?.email ?? '',
      phone: contact?.phone ?? '',
      note: contact?.note ?? '',
      isPrimary: contact?.isPrimary ?? false,
    },
  })

  useUnsavedChangesWarning(isDirty)

  const close = () => {
    if (isDirty && !window.confirm(t.common.unsavedChanges)) return
    onClose()
  }

  const onSubmit = async (values: ContactFormValues) => {
    const body = toContactRequest(values)
    if (contact) {
      await update.mutateAsync({ contactId: contact.id, body: { ...body, version: contact.version } })
    } else {
      await create.mutateAsync(body)
    }
    onClose()
  }

  return (
    <Modal
      title={contact ? t.contacts.editTitle : t.contacts.new}
      onClose={close}
      footer={
        <>
          <button type="button" className="secondary" onClick={close}>
            {t.common.cancel}
          </button>
          <button type="submit" form={FORM_ID} aria-busy={save.isPending} disabled={save.isPending}>
            {save.isPending ? t.common.saving : t.common.save}
          </button>
        </>
      }
    >
      <form id={FORM_ID} onSubmit={handleSubmit((values) => onSubmit(values).catch(() => {}))} noValidate>
        <div className="grid">
          <FormField label={t.contacts.fields.firstName} error={errors.firstName?.message}>
            <input {...register('firstName')} aria-invalid={errors.firstName ? true : undefined} autoFocus />
          </FormField>
          <FormField label={t.contacts.fields.lastName} error={errors.lastName?.message}>
            <input {...register('lastName')} aria-invalid={errors.lastName ? true : undefined} />
          </FormField>
        </div>

        <FormField label={t.contacts.fields.position} error={errors.position?.message}>
          <input {...register('position')} aria-invalid={errors.position ? true : undefined} />
        </FormField>

        <div className="grid">
          <FormField label={t.contacts.fields.email} error={errors.email?.message}>
            <input type="email" {...register('email')} aria-invalid={errors.email ? true : undefined} />
          </FormField>
          <FormField label={t.contacts.fields.phone} error={errors.phone?.message}>
            <input type="tel" {...register('phone')} aria-invalid={errors.phone ? true : undefined} />
          </FormField>
        </div>

        <FormField label={t.contacts.fields.note} error={errors.note?.message}>
          <textarea rows={3} {...register('note')} aria-invalid={errors.note ? true : undefined} />
        </FormField>

        <label>
          <input type="checkbox" {...register('isPrimary')} />
          {t.contacts.primary}
        </label>

        {save.error && <p role="alert">{errorMessage(save.error)}</p>}
      </form>
    </Modal>
  )
}