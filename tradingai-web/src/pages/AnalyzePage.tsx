import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import {
  Loader2,
  Upload,
  ImagePlus,
  ImageIcon,
  LineChart,
  AlertTriangle,
  ChevronDown,
  ChevronUp,
} from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Card, CardContent } from '@/components/ui/card'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

import { analysisApi } from '@/api/analysis'
import { assetsApi } from '@/api/assets'
import { useApiErrorToast } from '@/lib/toastError'
import {
  analyzeAssetSchema,
  analyzeImageSchema,
  TIMEFRAMES,
  type AnalyzeAssetFormValues,
  type AnalyzeImageFormValues,
} from '@/features/analysis/schemas'
import {
  buildProfilePrompt,
  useTradeProfile,
} from '@/features/analysis/tradeProfile'
import { TradeProfilePanel } from '@/components/analysis/TradeProfilePanel'
import { HowToUseDialog } from '@/components/analysis/HowToUseDialog'

export function AnalyzePage() {
  return (
    <div className="space-y-4">
      {/* Page header */}
      <div className="flex items-start justify-between gap-3 flex-wrap">
        <div className="space-y-1">
          <h1 className="text-2xl md:text-3xl font-bold flex items-center gap-2">
            <LineChart className="size-7 text-[#a855f7]" />
            Trade Analysis
          </h1>
          <p className="text-sm text-muted-foreground">
            Upload a chart to get started
          </p>
        </div>
        <HowToUseDialog />
      </div>

      {/* Two-column layout — main work area left, profile panel right */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2">
          <Tabs defaultValue="image" className="w-full">
            <TabsList className="grid w-full grid-cols-2 max-w-sm">
              <TabsTrigger value="image">
                <ImageIcon className="size-4 mr-1" /> Upload Image
              </TabsTrigger>
              <TabsTrigger value="asset">
                <LineChart className="size-4 mr-1" /> Tracked Asset
              </TabsTrigger>
            </TabsList>

            <TabsContent value="image" className="mt-4">
              <ImageAnalyzeForm />
            </TabsContent>
            <TabsContent value="asset" className="mt-4">
              <AssetAnalyzeForm />
            </TabsContent>
          </Tabs>
        </div>

        <aside className="lg:col-span-1">
          <TradeProfilePanel />
        </aside>
      </div>

      {/* Disclaimer banner */}
      <DisclaimerBanner />
    </div>
  )
}

// ---------------------------------------------------------------------------
function DisclaimerBanner() {
  const [open, setOpen] = useState(false)
  return (
    <div className="rounded-lg border border-amber-500/30 bg-amber-500/5">
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="w-full flex items-center justify-between gap-2 p-3 text-left"
      >
        <div className="flex items-center gap-2">
          <AlertTriangle className="size-4 text-amber-500" />
          <span className="text-sm font-medium text-amber-100">
            Not Financial Advice
          </span>
          <span className="text-sm text-muted-foreground hidden sm:inline">
            — Analysis is for educational purposes only.
          </span>
        </div>
        {open ? <ChevronUp className="size-4" /> : <ChevronDown className="size-4" />}
      </button>
      {open && (
        <div className="px-3 pb-3 text-xs text-muted-foreground">
          Trendox AI generates trade ideas from charts and market data using a
          large language model. The model can be wrong, miss context, or
          produce contradictory levels. Always validate with your own analysis
          before acting on any output.
        </div>
      )}
    </div>
  )
}

// ===========================================================================
// IMAGE-UPLOAD FORM — big drop zone, Asset Pair + Timeframe + Notes underneath
// ===========================================================================
function ImageAnalyzeForm() {
  const navigate = useNavigate()
  const toastError = useApiErrorToast({ redirectOn429: true })
  const { profile } = useTradeProfile()
  const [file, setFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)
  const [dragOver, setDragOver] = useState(false)

  const form = useForm<AnalyzeImageFormValues>({
    resolver: zodResolver(analyzeImageSchema),
    defaultValues: { assetPair: '', timeFrame: '1h', userPrompt: '' },
  })

  const mutation = useMutation({
    mutationFn: analysisApi.analyzeImage,
    onSuccess: (data) => {
      toast.success('Analysis complete')
      navigate(`/analyses/${data.id}`)
    },
    onError: (err) => toastError(err, 'Analysis failed'),
  })

  const handleFile = (f: File | null) => {
    setFile(f)
    if (previewUrl) URL.revokeObjectURL(previewUrl)
    setPreviewUrl(f ? URL.createObjectURL(f) : null)
  }

  const onSubmit = (values: AnalyzeImageFormValues) => {
    if (!file) {
      toast.error('Please upload a chart image')
      return
    }
    const userPrompt =
      profile.useInAnalysis && (values.userPrompt ?? '').trim().length === 0
        ? buildProfilePrompt(profile)
        : profile.useInAnalysis
        ? `${values.userPrompt}\n\n${buildProfilePrompt(profile)}`
        : values.userPrompt
    mutation.mutate({ ...values, file, userPrompt })
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        {/* Big drop zone */}
        <Card>
          <CardContent className="p-3">
            <label
              onDragOver={(e) => {
                e.preventDefault()
                setDragOver(true)
              }}
              onDragLeave={() => setDragOver(false)}
              onDrop={(e) => {
                e.preventDefault()
                setDragOver(false)
                const f = e.dataTransfer.files?.[0]
                if (f) handleFile(f)
              }}
              className={`border-2 border-dashed rounded-xl flex items-center justify-center cursor-pointer transition-colors block ${
                dragOver
                  ? 'border-[#a855f7] bg-[#a855f7]/5'
                  : 'border-border hover:border-[#a855f7]/50'
              }`}
              style={{ minHeight: 360 }}
            >
              <input
                type="file"
                accept="image/*"
                className="hidden"
                onChange={(e) => handleFile(e.target.files?.[0] ?? null)}
              />
              {previewUrl ? (
                <div className="p-4 w-full">
                  <img
                    src={previewUrl}
                    alt="Preview"
                    className="max-h-[400px] mx-auto rounded-md"
                  />
                </div>
              ) : (
                <div className="text-center p-8 space-y-3">
                  <div className="size-16 rounded-2xl bg-[#a855f7]/15 flex items-center justify-center mx-auto">
                    <Upload className="size-7 text-[#a855f7]" />
                  </div>
                  <div className="space-y-1">
                    <p className="text-lg font-semibold">Upload Chart Image</p>
                    <p className="text-sm text-muted-foreground">
                      Drag &amp; drop or click to browse
                    </p>
                  </div>
                  <Button
                    type="button"
                    className="bg-[#a855f7] hover:bg-[#9333ea] mt-2"
                    onClick={(e) => {
                      e.preventDefault()
                      ;(e.currentTarget.parentElement?.parentElement
                        ?.querySelector('input[type="file"]') as HTMLInputElement)?.click()
                    }}
                  >
                    <ImagePlus className="size-4 mr-2" /> Select Image
                  </Button>
                  <p className="text-xs text-muted-foreground">PNG, JPG, or WEBP</p>
                </div>
              )}
            </label>
            {file && (
              <p className="text-xs text-muted-foreground mt-2 text-center">
                {file.name} · {(file.size / 1024).toFixed(0)} KB ·{' '}
                <button
                  type="button"
                  className="text-destructive hover:underline"
                  onClick={() => handleFile(null)}
                >
                  remove
                </button>
              </p>
            )}
          </CardContent>
        </Card>

        {/* Pair + timeframe + notes */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <FormField
            control={form.control}
            name="assetPair"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Asset pair</FormLabel>
                <FormControl>
                  <Input placeholder="BTC/USDT" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="timeFrame"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Timeframe</FormLabel>
                <Select onValueChange={field.onChange} value={field.value}>
                  <FormControl>
                    <SelectTrigger className="w-full">
                      <SelectValue />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {TIMEFRAMES.map((tf) => (
                      <SelectItem key={tf} value={tf}>
                        {tf}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="userPrompt"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Notes (optional)</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Any context the AI should know?"
                  rows={3}
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button
          type="submit"
          className="w-full h-12 bg-gradient-to-r from-[#7c3aed] to-[#3b82f6] hover:opacity-90 text-base font-semibold rounded-xl"
          disabled={mutation.isPending || !file}
        >
          {mutation.isPending ? (
            <>
              <Loader2 className="size-4 mr-2 animate-spin" />
              Analyzing... (this can take 10-15s)
            </>
          ) : (
            'Run analysis'
          )}
        </Button>
      </form>
    </Form>
  )
}

// ===========================================================================
// TRACKED-ASSET FORM — picker + timeframe + notes (no upload)
// ===========================================================================
function AssetAnalyzeForm() {
  const navigate = useNavigate()
  const toastError = useApiErrorToast({ redirectOn429: true })
  const { profile } = useTradeProfile()

  const { data: assets, isLoading: assetsLoading } = useQuery({
    queryKey: ['assets'],
    queryFn: () => assetsApi.list(),
  })

  const form = useForm<AnalyzeAssetFormValues>({
    resolver: zodResolver(analyzeAssetSchema),
    defaultValues: { assetId: '', timeFrame: '1h', userPrompt: '' },
  })

  const mutation = useMutation({
    mutationFn: analysisApi.analyzeAsset,
    onSuccess: (data) => {
      toast.success('Analysis complete')
      navigate(`/analyses/${data.id}`)
    },
    onError: (err) => toastError(err, 'Analysis failed'),
  })

  const onSubmit = (values: AnalyzeAssetFormValues) => {
    const userPrompt =
      profile.useInAnalysis && (values.userPrompt ?? '').trim().length === 0
        ? buildProfilePrompt(profile)
        : profile.useInAnalysis
        ? `${values.userPrompt}\n\n${buildProfilePrompt(profile)}`
        : values.userPrompt
    mutation.mutate({ ...values, userPrompt })
  }

  return (
    <Card>
      <CardContent className="p-4">
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="assetId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Asset</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue
                          placeholder={assetsLoading ? 'Loading...' : 'Pick an asset'}
                        />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {assets?.map((a) => (
                        <SelectItem key={a.id} value={a.id}>
                          {a.pair} — {a.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="timeFrame"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Timeframe</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {TIMEFRAMES.map((tf) => (
                        <SelectItem key={tf} value={tf}>
                          {tf}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="userPrompt"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Notes (optional)</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="e.g. 'Focus on volume divergence'"
                      rows={3}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Button
              type="submit"
              className="w-full h-12 bg-gradient-to-r from-[#7c3aed] to-[#3b82f6] hover:opacity-90 text-base font-semibold rounded-xl"
              disabled={mutation.isPending}
            >
              {mutation.isPending ? (
                <>
                  <Loader2 className="size-4 mr-2 animate-spin" />
                  Analyzing... (this can take 10-15s)
                </>
              ) : (
                'Run analysis'
              )}
            </Button>
          </form>
        </Form>
      </CardContent>
    </Card>
  )
}
