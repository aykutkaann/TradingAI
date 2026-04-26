import { useState } from 'react'
import { Mail, MessageSquare, Send } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useAuthStore } from '@/stores/authStore'

const REASONS = [
  'Bug report',
  'Billing issue',
  'Feature request',
  'Account help',
  'AI analysis was incorrect',
  'Other',
]

/**
 * Placeholder support form. Until we have a backend endpoint that emails
 * support@trendoxai.com (or routes to a help-desk like Crisp / Plain),
 * this page just collects the input and shows a confirmation toast.
 *
 * To wire it up: add POST /api/support that emails the form contents
 * via the existing IEmailService.
 */
export function SupportPage() {
  const user = useAuthStore((s) => s.user)
  const [reason, setReason] = useState('')
  const [email, setEmail] = useState(user?.email ?? '')
  const [subject, setSubject] = useState('')
  const [message, setMessage] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!reason || !email || !subject || !message) {
      toast.error('Please fill in all fields')
      return
    }
    setSubmitting(true)
    // TODO: POST /api/support — backend endpoint not yet implemented.
    // For now we fake a network call so the UX is testable.
    await new Promise((r) => setTimeout(r, 600))
    setSubmitting(false)
    toast.success('Thanks — we received your request. Reply usually within 24h.')
    setReason('')
    setSubject('')
    setMessage('')
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl md:text-3xl font-bold flex items-center gap-2">
          <MessageSquare className="size-6 text-[#a855f7]" />
          Support &amp; Feedback
        </h1>
        <p className="text-muted-foreground">
          We're here to help. Send us your questions, feedback, or report any issues
          you're experiencing.
        </p>
      </div>

      <Card>
        <CardContent className="p-6 space-y-4">
          <div className="flex items-center gap-2 text-sm font-semibold">
            <Mail className="size-4 text-[#a855f7]" /> Contact Support
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-1.5">
              <Label className="text-xs">Reason for Contact *</Label>
              <Select value={reason} onValueChange={setReason}>
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select a reason..." />
                </SelectTrigger>
                <SelectContent>
                  {REASONS.map((r) => (
                    <SelectItem key={r} value={r}>
                      {r}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-1.5">
              <Label className="text-xs">Your Email Address *</Label>
              <Input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="your.email@example.com"
              />
              <p className="text-[11px] text-muted-foreground">
                We'll use this email to respond to your request.
              </p>
            </div>

            <div className="space-y-1.5">
              <Label className="text-xs">Subject *</Label>
              <Input
                value={subject}
                onChange={(e) => setSubject(e.target.value)}
                placeholder="Brief description of your request..."
              />
            </div>

            <div className="space-y-1.5">
              <Label className="text-xs">Message *</Label>
              <Textarea
                rows={6}
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                placeholder="Please provide detailed information about your request, including any steps to reproduce issues..."
                maxLength={2000}
              />
              <p className="text-[11px] text-muted-foreground text-right">
                {message.length} / 2000
              </p>
            </div>

            <Button
              type="submit"
              className="w-full h-11 bg-[#a855f7] hover:bg-[#9333ea]"
              disabled={submitting}
            >
              <Send className="size-4 mr-2" />
              {submitting ? 'Sending…' : 'Send Support Request'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
