import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { errorMessage } from '../../api/errors'
import type { CustomerDetail } from '../../api/types'
import FormField from '../../components/FormField'
import Modal from '../../components/Modal'
import { useUnsavedChangesWarning } from '../../hooks/useUnsavedChangesWarning'
import { t } from '../../i18n'
import { customerSchema, toCustomerRequest, type CustomerFormValues } from './customerSchema'
import { useCreateCustomer, useUpdateCustomer } from './queries'

type CustomerFormDialogProps = {
  /** Existing customer to edit. Without it, the form creates a new customer. */
  customer?: CustomerDetail
  onClose: () => void
  onSaved?: (customer: CustomerDetail) => void
}

const FORM_ID = 'customer-form'

export default function CustomerFormDialog({ customer, onClose, onSaved }: CustomerFormDialogProps) {
  const create = useCreateCustomer()
  const update = useUpdateCustomer(customer?.id ?? 0)
  const save = customer ? update : create

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
  } = useForm<CustomerFormValues>({
    resolver: zodResolver(customerSchema),
    defaultValues: {
      name: customer?.name ?? '',
      email: customer?.email ?? '',
      phone: customer?.phone ?? '',
      street: customer?.street ?? '',
      postalCode: customer?.postalCode ?? '',
      city: customer?.city ?? '',
      note: customer?.note ?? '',
    },
  })

  useUnsavedChangesWarning(isDirty)

  const close = () => {
    if (isDirty && !window.confirm(t.common.unsavedChanges)) return
    onClose()
  }

  const onSubmit = async (values: CustomerFormValues) => {
    const body = toCustomerRequest(values)
    const saved = customer
      ? await update.mutateAsync({ ...body, isActive: customer.isActive, version: customer.version })
      : await create.mutateAsync(body)
    onSaved?.(saved)
    onClose()
  }

  return (
    <Modal
      title={customer ? t.customers.editTitle : t.customers.newTitle}
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
      {/* handleSubmit runs the zod rules first and only calls onSubmit when everything is valid */}
      <form id={FORM_ID} onSubmit={handleSubmit((values) => onSubmit(values).catch(() => {}))} noValidate>
        <FormField label={t.customers.fields.name} error={errors.name?.message}>
          <input {...register('name')} aria-invalid={errors.name ? true : undefined} autoFocus />
        </FormField>

        <div className="grid">
          <FormField label={t.customers.fields.email} error={errors.email?.message}>
            <input type="email" {...register('email')} aria-invalid={errors.email ? true : undefined} />
          </FormField>
          <FormField label={t.customers.fields.phone} error={errors.phone?.message}>
            <input type="tel" {...register('phone')} aria-invalid={errors.phone ? true : undefined} />
          </FormField>
        </div>

        <FormField label={t.customers.fields.street} error={errors.street?.message}>
          <input {...register('street')} aria-invalid={errors.street ? true : undefined} />
        </FormField>

        <div className="grid">
          <FormField label={t.customers.fields.postalCode} error={errors.postalCode?.message}>
            <input {...register('postalCode')} aria-invalid={errors.postalCode ? true : undefined} />
          </FormField>
          <FormField label={t.customers.fields.city} error={errors.city?.message}>
            <input {...register('city')} aria-invalid={errors.city ? true : undefined} />
          </FormField>
        </div>

        <FormField label={t.customers.fields.note} error={errors.note?.message}>
          <textarea rows={3} {...register('note')} aria-invalid={errors.note ? true : undefined} />
        </FormField>

        {save.error && <p role="alert">{errorMessage(save.error)}</p>}
      </form>
    </Modal>
  )
}