import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'review_models.dart';
import 'reviews_bloc.dart';
import 'reviews_event.dart';
import 'reviews_state.dart';
import 'star_rating_widget.dart';

class ReviewsScreen extends StatelessWidget {
  const ReviewsScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Reviews')),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _showWriteReview(context),
        child: const Icon(Icons.rate_review),
      ),
      body: BlocBuilder<ReviewsBloc, ReviewsState>(
        builder: (context, state) {
          return state.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            loaded: (reviews, rating, hasMore, _) =>
                _ReviewsContent(
                  hubId: hubId,
                  reviews: reviews,
                  rating: rating,
                  hasMore: hasMore,
                ),
            submitting: () => const Center(child: CircularProgressIndicator()),
            error: (message) => Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('Error: $message'),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => context
                        .read<ReviewsBloc>()
                        .add(ReviewsEvent.load(hubId: hubId)),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  void _showWriteReview(BuildContext context) {
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (ctx) => _WriteReviewSheet(hubId: hubId, parentContext: context),
    );
  }
}

class _ReviewsContent extends StatelessWidget {
  const _ReviewsContent({
    required this.hubId,
    required this.reviews,
    required this.rating,
    required this.hasMore,
  });

  final String hubId;
  final List<HubReview> reviews;
  final HubRating rating;
  final bool hasMore;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return CustomScrollView(
      slivers: [
        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Row(
                  children: [
                    Column(
                      children: [
                        Text(
                          rating.averageRating.toStringAsFixed(1),
                          style: theme.textTheme.headlineLarge
                              ?.copyWith(fontWeight: FontWeight.bold),
                        ),
                        StarRatingWidget(
                          rating: rating.averageRating.round(),
                          size: 20,
                        ),
                        Text(
                          '${rating.totalReviews} reviews',
                          style: theme.textTheme.bodySmall,
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
        if (reviews.isEmpty)
          const SliverFillRemaining(
            child: Center(child: Text('No reviews yet')),
          )
        else
          SliverList(
            delegate: SliverChildBuilderDelegate(
              (context, index) {
                if (index == reviews.length) {
                  if (hasMore) {
                    context
                        .read<ReviewsBloc>()
                        .add(ReviewsEvent.loadMore(hubId: hubId));
                    return const Padding(
                      padding: EdgeInsets.all(16),
                      child: Center(child: CircularProgressIndicator()),
                    );
                  }
                  return null;
                }
                return _ReviewTile(review: reviews[index]);
              },
              childCount: reviews.length + (hasMore ? 1 : 0),
            ),
          ),
      ],
    );
  }
}

class _ReviewTile extends StatelessWidget {
  const _ReviewTile({required this.review});

  final HubReview review;

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: CircleAvatar(
        backgroundImage:
            review.avatar != null ? NetworkImage(review.avatar!) : null,
        child: review.avatar == null
            ? Text(review.displayName[0].toUpperCase())
            : null,
      ),
      title: Row(
        children: [
          Text(review.displayName),
          const SizedBox(width: 8),
          StarRatingWidget(rating: review.rating, size: 14),
        ],
      ),
      subtitle: review.comment != null ? Text(review.comment!) : null,
    );
  }
}

class _WriteReviewSheet extends StatefulWidget {
  const _WriteReviewSheet({
    required this.hubId,
    required this.parentContext,
  });

  final String hubId;
  final BuildContext parentContext;

  @override
  State<_WriteReviewSheet> createState() => _WriteReviewSheetState();
}

class _WriteReviewSheetState extends State<_WriteReviewSheet> {
  int _rating = 0;
  final _commentController = TextEditingController();

  @override
  void dispose() {
    _commentController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.fromLTRB(
        16,
        16,
        16,
        MediaQuery.of(context).viewInsets.bottom + 16,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Write a Review',
              style: Theme.of(context).textTheme.titleLarge),
          const SizedBox(height: 16),
          Center(
            child: StarRatingWidget(
              rating: _rating,
              size: 40,
              onRatingChanged: (r) => setState(() => _rating = r),
            ),
          ),
          const SizedBox(height: 16),
          TextField(
            controller: _commentController,
            decoration: const InputDecoration(
              hintText: 'Share your experience...',
              border: OutlineInputBorder(),
            ),
            maxLines: 3,
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: _rating > 0 ? _submit : null,
              child: const Text('Submit Review'),
            ),
          ),
        ],
      ),
    );
  }

  void _submit() {
    widget.parentContext.read<ReviewsBloc>().add(
          ReviewsEvent.submit(
            hubId: widget.hubId,
            request: CreateReviewRequest(
              rating: _rating,
              comment: _commentController.text.trim().isNotEmpty
                  ? _commentController.text.trim()
                  : null,
            ),
          ),
        );
    Navigator.of(context).pop();
  }
}
